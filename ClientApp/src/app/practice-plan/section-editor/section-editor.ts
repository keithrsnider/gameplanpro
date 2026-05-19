import { moveItemInArray } from '@angular/cdk/drag-drop';
import { CdkDrag, CdkDropList } from '@angular/cdk/drag-drop';
import type { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, effect, inject, input, type OnDestroy, signal } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { ApiClientService } from '../../core/api-client.service';
import type { BulkUpdateSectionDisplayOrderRequest, SectionResponse } from '../../core/api/models';
import type { EditableSection } from '../practice-plan-form.types';
import { SectionCardComponent } from '../section-card/section-card';

const SECTION_SAVE_DEBOUNCE_MS = 700;
const SECTION_REORDER_DEBOUNCE_MS = 450;

@Component({
	selector: 'gpp-section-editor',
	templateUrl: './section-editor.html',
	styleUrl: './section-editor.css',
	imports: [CdkDropList, CdkDrag, SectionCardComponent, ...HlmButtonImports, ...HlmIconImports],
	standalone: true,
})
export class SectionEditorComponent implements OnDestroy {
	private readonly _api = inject(ApiClientService);

	readonly planKey = input<string | null>(null);
	readonly loading = input<boolean>(false);
	readonly planSaveMessage = input<string | null>(null);
	readonly initialSections = input<EditableSection[]>([]);

	readonly canManageSections = signal(false);
	readonly sections = signal<EditableSection[]>([]);
	readonly creatingSection = signal(false);
	readonly sectionOrderPending = signal(false);
	readonly sectionOrderSaving = signal(false);
	readonly sectionOrderError = signal<string | null>(null);

	private readonly _sectionSaveTimers = new Map<string, number>();
	private _sectionOrderSaveTimer: number | null = null;
	private _pendingOrderRollback: Map<string, number> | null = null;

	constructor() {
		effect(() => {
			const key = this.planKey();
			this.canManageSections.set(!!key);
		});

		effect(() => {
			const initial = this.initialSections();
			if (initial.length > 0) {
				this.sections.set(initial);
			}
		});
	}

	ngOnDestroy() {
		for (const timer of this._sectionSaveTimers.values()) {
			window.clearTimeout(timer);
		}
		if (this._sectionOrderSaveTimer !== null) {
			window.clearTimeout(this._sectionOrderSaveTimer);
		}
	}

	drillCount(section: EditableSection): number {
		return section.planDrills.length;
	}

	durationMinutes(section: EditableSection): number {
		return section.planDrills.reduce((total, drill) => total + (drill.duration ?? 0), 0);
	}

	requestAddSection() {
		void this.addSection();
	}

	onSectionDrop(event: CdkDragDrop<EditableSection[]>) {
		this.dropSection(event);
	}

	onSectionNameInput(sectionKey: string, value: string) {
		this.updateSection(sectionKey, { name: value, errorMessage: null });
		this.scheduleSectionSave(sectionKey);
	}

	onSectionNoteInput(sectionKey: string, value: string) {
		this.updateSection(sectionKey, { note: value, errorMessage: null });
		this.scheduleSectionSave(sectionKey);
	}

	retrySectionSave(sectionKey: string) {
		void this.persistSection(sectionKey);
	}

	private async addSection() {
		const planKey = this.planKey();
		if (!planKey || this.creatingSection()) return;

		this.creatingSection.set(true);
		this.sectionOrderError.set(null);

		try {
			const displayOrder = this.sections().length + 1;
			const created = await this._api.client.api.practicePlans
				.byKeyId(planKey)
				.sections.post({
					displayOrder,
					name: `Section ${displayOrder}`,
					note: null,
				});

			if (!created?.key) {
				this.sectionOrderError.set('Failed to add section. Please try again.');
				return;
			}

			this.sections.update((sections) =>
				this.normalizeSections([
					...sections,
					this.toEditableSection(created, displayOrder),
				])
			);
		} catch {
			this.sectionOrderError.set('Failed to add section. Please try again.');
		} finally {
			this.creatingSection.set(false);
		}
	}

	async deleteSection(sectionKey: string) {
		const planKey = this.planKey();
		if (!planKey) return;

		const section = this.sections().find((item) => item.key === sectionKey);
		if (!section) return;

		const confirmed = window.confirm(`Delete ${section.name || 'this section'}?`);
		if (!confirmed) return;

		this.updateSection(sectionKey, { saveState: 'saving', errorMessage: null });

		try {
			await this._api.client.api.practicePlans
				.byKeyId(planKey)
				.sections.bySectionKey(sectionKey)
				.delete();

			const remaining = this.sections().filter((item) => item.key !== sectionKey);
			const normalized = this.normalizeSections(remaining);
			const orderChanged = normalized.some(
				(item, index) => item.displayOrder !== remaining[index]?.displayOrder
			);

			this.sections.set(normalized);

			if (orderChanged) {
				this.queueSectionOrderSave(false);
			}
		} catch {
			this.updateSection(sectionKey, {
				saveState: 'error',
				errorMessage: 'Failed to delete section. Please try again.',
			});
		}
	}

	private dropSection(event: CdkDragDrop<EditableSection[]>) {
		if (event.previousIndex === event.currentIndex) return;

		if (!this._pendingOrderRollback) {
			this._pendingOrderRollback = new Map(
				this.sections().map((section) => [section.key, section.displayOrder])
			);
		}

		const reordered = this.cloneSections(this.sections());
		moveItemInArray(reordered, event.previousIndex, event.currentIndex);
		this.sections.set(this.normalizeSections(reordered));
		this.queueSectionOrderSave(true);
	}

	private scheduleSectionSave(sectionKey: string) {
		const existingTimer = this._sectionSaveTimers.get(sectionKey);
		if (existingTimer) window.clearTimeout(existingTimer);

		const timer = window.setTimeout(() => {
			this._sectionSaveTimers.delete(sectionKey);
			void this.persistSection(sectionKey);
		}, SECTION_SAVE_DEBOUNCE_MS);

		this._sectionSaveTimers.set(sectionKey, timer);
	}

	private async persistSection(sectionKey: string) {
		const planKey = this.planKey();
		if (!planKey) return;

		const section = this.sections().find((item) => item.key === sectionKey);
		if (!section) return;

		this.updateSection(sectionKey, { saveState: 'saving', errorMessage: null });

		try {
			const savedSection = await this._api.client.api.practicePlans
				.byKeyId(planKey)
				.sections.bySectionKey(sectionKey)
				.put({
					displayOrder: section.displayOrder,
					name: section.name.trim() || 'Untitled Section',
					note: section.note || null,
				});

			if (!savedSection?.key) {
				this.updateSection(sectionKey, {
					saveState: 'error',
					errorMessage: 'Failed to save section changes. Retry or keep editing.',
				});
				return;
			}

			this.sections.update((sections) =>
				this.normalizeSections(
					sections.map((item) =>
						item.key === sectionKey
							? {
								...item,
								...this.toEditableSection(savedSection, item.displayOrder),
								saveState: 'idle',
								errorMessage: null,
							}
							: item
					)
				)
			);
		} catch {
			this.updateSection(sectionKey, {
				saveState: 'error',
				errorMessage: 'Failed to save section changes. Retry or keep editing.',
			});
		}
	}

	private queueSectionOrderSave(trackRollback: boolean) {
		if (!this.planKey()) return;

		if (trackRollback && !this._pendingOrderRollback) {
			this._pendingOrderRollback = new Map(
				this.sections().map((section) => [section.key, section.displayOrder])
			);
		}

		if (this._sectionOrderSaveTimer !== null) {
			window.clearTimeout(this._sectionOrderSaveTimer);
		}

		this.sectionOrderPending.set(true);
		this.sectionOrderError.set(null);

		this._sectionOrderSaveTimer = window.setTimeout(() => {
			this._sectionOrderSaveTimer = null;
			void this.persistSectionOrder();
		}, SECTION_REORDER_DEBOUNCE_MS);
	}

	private async persistSectionOrder() {
		const planKey = this.planKey();
		if (!planKey) return;

		const orderPayload: BulkUpdateSectionDisplayOrderRequest[] = this.sections().map(
			(section) => ({
				displayOrder: section.displayOrder,
				sectionKey: section.key,
			})
		);

		if (!orderPayload.length) {
			this.sectionOrderPending.set(false);
			this.sectionOrderSaving.set(false);
			this.sectionOrderError.set(null);
			this._pendingOrderRollback = null;
			return;
		}

		this.sectionOrderPending.set(false);
		this.sectionOrderSaving.set(true);

		try {
			await this._api.client.api.practicePlans
				.byKeyId(planKey)
				.sections.order.put(orderPayload);

			this.sectionOrderError.set(null);
			this._pendingOrderRollback = null;
		} catch {
			if (this._pendingOrderRollback) {
				const rollbackOrder = this._pendingOrderRollback;
				this.sections.update((sections) =>
					this.normalizeSections(
						sections
							.map((section) => ({
								...section,
								displayOrder: rollbackOrder.get(section.key) ?? Number.MAX_SAFE_INTEGER,
							}))
							.sort((a, b) => a.displayOrder - b.displayOrder)
					)
				);
				this.sectionOrderError.set(
					'Failed to save section order. The previous arrangement was restored.'
				);
			} else {
				this.sectionOrderError.set(
					'Failed to sync section order. Please try moving the section again.'
				);
			}
		} finally {
			this.sectionOrderSaving.set(false);
			this._pendingOrderRollback = null;
		}
	}

	private updateSection(sectionKey: string, patch: Partial<EditableSection>) {
		this.sections.update((sections) =>
			sections.map((section) =>
				section.key === sectionKey ? { ...section, ...patch } : section
			)
		);
	}

	private normalizeSections(sections: EditableSection[]): EditableSection[] {
		return [...sections]
			.sort((a, b) => a.displayOrder - b.displayOrder)
			.map((section, index) => ({ ...section, displayOrder: index + 1 }));
	}

	private cloneSections(sections: EditableSection[]): EditableSection[] {
		return sections.map((section) => ({ ...section, planDrills: [...section.planDrills] }));
	}

	toEditableSection(section: SectionResponse, fallbackDisplayOrder: number): EditableSection {
		return {
			key: String(section.key ?? ''),
			name: section.name ?? '',
			note: section.note ?? '',
			displayOrder: section.displayOrder ?? fallbackDisplayOrder,
			planDrills: [...(section.planDrills ?? [])],
			saveState: 'idle',
			errorMessage: null,
		};
	}
}
