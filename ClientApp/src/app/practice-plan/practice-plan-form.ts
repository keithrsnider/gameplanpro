import {
	CdkDrag,
	CdkDragHandle,
	CdkDropList,
	moveItemInArray,
} from '@angular/cdk/drag-drop';
import type { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Component, inject, signal } from '@angular/core';
import type { OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
	form,
	FormField,
	maxLength,
	required,
	schema,
	submit,
} from '@angular/forms/signals';
import type { FieldTree } from '@angular/forms/signals';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { BrnSelectImports } from '@spartan-ng/brain/select';
import { FormErrorsComponent } from '../shared/components/form-errors';
import { HorizontalRuleComponent } from '../shared/components/horizontal-rule';
import { ApiClientService } from '../core/api-client.service';
import type {
	BulkUpdateSectionDisplayOrderRequest,
	PlanDrillResponse,
	SectionResponse,
} from '../core/api/models';

interface PracticePlanFormData {
	name: string;
	description: string;
}

type SectionSaveState = 'idle' | 'saving' | 'error';

interface EditableSection {
	key: string;
	name: string;
	note: string;
	displayOrder: number;
	planDrills: PlanDrillResponse[];
	saveState: SectionSaveState;
	errorMessage: string | null;
}

const practicePlanSchema = schema<PracticePlanFormData>((f) => {
	required(f.name, { message: 'Plan name is required.' });
	maxLength(f.name, 200, { message: 'Name must be 200 characters or fewer.' });
	maxLength(f.description, 2000, {
		message: 'Description must be 2000 characters or fewer.',
	});
});

const DURATION_OPTIONS = [30, 45, 60, 75, 90, 105, 120];
const SECTION_SAVE_DEBOUNCE_MS = 700;
const SECTION_REORDER_DEBOUNCE_MS = 450;

@Component({
	selector: 'gpp-practice-plan-form',
	templateUrl: './practice-plan-form.html',
	styleUrl: './practice-plan-form.css',
	imports: [
		CdkDropList,
		CdkDrag,
		CdkDragHandle,
		RouterLink,
		FormField,
		FormErrorsComponent,
		HorizontalRuleComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
		...HlmSelectImports,
		...HlmTextareaImports,
		...HlmIconImports,
		...BrnSelectImports,
	],
})
export class PracticePlanFormComponent implements OnDestroy {
	private readonly _router = inject(Router);
	private readonly _route = inject(ActivatedRoute);
	private readonly _api = inject(ApiClientService);

	readonly isEditMode: boolean;
	readonly planKey: string | null;
	readonly durations = DURATION_OPTIONS;

	readonly model = signal<PracticePlanFormData>({ name: '', description: '' });
	readonly planForm = form(this.model, practicePlanSchema);
	readonly sections = signal<EditableSection[]>([]);

	readonly selectedLocation = signal<string>('');
	readonly selectedDuration = signal<string>('');
	readonly creatingSection = signal(false);
	readonly sectionOrderPending = signal(false);
	readonly sectionOrderSaving = signal(false);
	readonly sectionOrderError = signal<string | null>(null);
	readonly planSaveMessage = signal<string | null>(null);

	apiErrors: string[] = [];
	loading = signal(false);

	private readonly _sectionSaveTimers = new Map<string, number>();
	private _sectionOrderSaveTimer: number | null = null;
	private _pendingOrderRollback: Map<string, number> | null = null;

	constructor() {
		const key = this._route.snapshot.paramMap.get('key');
		this.isEditMode = key !== null && key !== 'new';
		this.planKey = this.isEditMode ? key : null;

		if (this.isEditMode && this.planKey) {
			this.loadPlan(this.planKey);
		}
	}

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	get canManageSections(): boolean {
		return !!this.planKey;
	}

	ngOnDestroy() {
		for (const timer of this._sectionSaveTimers.values()) {
			window.clearTimeout(timer);
		}

		if (this._sectionOrderSaveTimer !== null) {
			window.clearTimeout(this._sectionOrderSaveTimer);
		}
	}

	async loadPlan(key: string) {
		this.loading.set(true);
		try {
			const plan = await this._api.client.api.practicePlans.byKeyId(key).get();
			if (!plan) {
				this.apiErrors = ['Failed to load practice plan.'];
				return;
			}

			this.model.set({
				name: plan.name ?? '',
				description: plan.description ?? '',
			});
			if (plan.location) this.selectedLocation.set(plan.location);
			if (plan.intendedDuration) {
				this.selectedDuration.set(String(plan.intendedDuration));
			}
			this.sections.set(this.mapSections(plan.sections));
		} catch {
			this.apiErrors = ['Failed to load practice plan.'];
		} finally {
			this.loading.set(false);
		}
	}

	sectionDrillCount(section: EditableSection): number {
		return section.planDrills.length;
	}

	sectionDurationMinutes(section: EditableSection): number {
		return section.planDrills.reduce((total, drill) => total + (drill.duration ?? 0), 0);
	}

	updateSelectedLocation(value: string | string[] | undefined) {
		this.selectedLocation.set(typeof value === 'string' ? value : '');
	}

	updateSelectedDuration(value: string | string[] | undefined) {
		this.selectedDuration.set(typeof value === 'string' ? value : '');
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

	async addSection() {
		if (!this.planKey || this.creatingSection()) {
			return;
		}

		this.creatingSection.set(true);
		this.sectionOrderError.set(null);

		try {
			const displayOrder = this.sections().length + 1;
			const created = await this._api.client.api.practicePlans
				.byKeyId(this.planKey)
				.sections.post({
					displayOrder,
					name: `Section ${displayOrder + 1}`,
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
		if (!this.planKey) {
			return;
		}

		const section = this.sections().find((item) => item.key === sectionKey);
		if (!section) {
			return;
		}

		const confirmed = window.confirm(`Delete ${section.name || 'this section'}?`);
		if (!confirmed) {
			return;
		}

		this.updateSection(sectionKey, { saveState: 'saving', errorMessage: null });

		try {
			await this._api.client.api.practicePlans
				.byKeyId(this.planKey)
				.sections.bySectionKey(sectionKey)
				.delete();

			const remaining = this.sections().filter((item) => item.key !== sectionKey);
			const normalized = this.normalizeSections(remaining);
			const orderChanged = normalized.some((item, index) => item.displayOrder !== remaining[index]?.displayOrder);

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

	dropSection(event: CdkDragDrop<EditableSection[]>) {
		if (event.previousIndex === event.currentIndex) {
			return;
		}

		const reordered = this.cloneSections(this.sections());

		if (!this._pendingOrderRollback) {
			this._pendingOrderRollback = new Map(
				reordered.map((section) => [section.key, section.displayOrder])
			);
		}

		moveItemInArray(reordered, event.previousIndex, event.currentIndex);
		this.sections.set(this.normalizeSections(reordered));
		this.queueSectionOrderSave(true);
	}

	async onSubmit() {
		this.apiErrors = [];
		this.planSaveMessage.set(null);

		await submit(this.planForm, async (f) => {
			const { name, description } = f().value();
			const body = {
				name,
				location: this.selectedLocation() || null,
				intendedDuration: this.selectedDuration()
					? Number(this.selectedDuration())
					: null,
				description: description || null,
			};

			try {
				if (this.isEditMode && this.planKey) {
					await this._api.client.api.practicePlans
						.byKeyId(this.planKey)
						.put(body);
					this.planSaveMessage.set('Practice plan details saved.');
				} else {
					const created = await this._api.client.api.practicePlans.post(body);
					if (created?.key) {
						await this._router.navigate(['/practice-plan', created.key]);
						return undefined;
					}

					await this._router.navigate(['/dashboard']);
					return undefined;
				}
			} catch {
				this.apiErrors = [
					this.isEditMode
						? 'Failed to save practice plan. Please try again.'
						: 'Failed to create practice plan. Please try again.',
				];
			}
			return undefined;
		});
	}

	private mapSections(sections: SectionResponse[] | null | undefined): EditableSection[] {
		return this.normalizeSections(
			(sections ?? []).map((section, index) => this.toEditableSection(section, index))
		);
	}

	private toEditableSection(
		section: SectionResponse,
		fallbackDisplayOrder: number
	): EditableSection {
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

	private normalizeSections(sections: EditableSection[]): EditableSection[] {
		return [...sections]
			.sort((a, b) => a.displayOrder - b.displayOrder)
			.map((section, index) => ({ ...section, displayOrder: index + 1 }));
	}

	private cloneSections(sections: EditableSection[]): EditableSection[] {
		return sections.map((section) => ({
			...section,
			planDrills: [...section.planDrills],
		}));
	}

	private updateSection(sectionKey: string, patch: Partial<EditableSection>) {
		this.sections.update((sections) =>
			sections.map((section) =>
				section.key === sectionKey ? { ...section, ...patch } : section
			)
		);
	}

	private scheduleSectionSave(sectionKey: string) {
		const existingTimer = this._sectionSaveTimers.get(sectionKey);
		if (existingTimer) {
			window.clearTimeout(existingTimer);
		}

		const timer = window.setTimeout(() => {
			this._sectionSaveTimers.delete(sectionKey);
			void this.persistSection(sectionKey);
		}, SECTION_SAVE_DEBOUNCE_MS);

		this._sectionSaveTimers.set(sectionKey, timer);
	}

	private async persistSection(sectionKey: string) {
		if (!this.planKey) {
			return;
		}

		const section = this.sections().find((item) => item.key === sectionKey);
		if (!section) {
			return;
		}

		this.updateSection(sectionKey, { saveState: 'saving', errorMessage: null });

		try {
			const savedSection = await this._api.client.api.practicePlans
				.byKeyId(this.planKey)
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
		if (!this.planKey) {
			return;
		}

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
		if (!this.planKey) {
			return;
		}

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
				.byKeyId(this.planKey)
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
								displayOrder:
									rollbackOrder.get(section.key) ?? Number.MAX_SAFE_INTEGER,
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
}
