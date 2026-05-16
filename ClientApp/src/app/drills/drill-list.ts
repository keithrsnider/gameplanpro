import { Component, computed, effect, inject, signal } from '@angular/core';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import type {
	CoachResponse,
	CreateDrillRequest,
	DrillResponse,
	DrillTypeResponse,
} from '../core/api/models';
import { ApiClientService } from '../core/api-client.service';
import { DrillFormComponent, type DrillFormValue } from '../shared/components/drill-form';
import { DrillListItemComponent } from './drill-list-item/drill-list-item';

type DrillSourceFilter = 'system' | 'user';

interface DrillTypeChip {
	id: number;
	name: string;
	count: number;
}

@Component({
	selector: 'gpp-drill-list',
	templateUrl: './drill-list.html',
	styleUrl: './drill-list.css',
	imports: [...HlmIconImports, ...HlmInputImports, DrillFormComponent, DrillListItemComponent],
})
export class DrillListComponent {
	private readonly _api = inject(ApiClientService);

	readonly isSystemSourceSelected = signal(true);
	readonly isUserSourceSelected = signal(true);
	readonly drills = signal<DrillResponse[]>([]);
	readonly drillTypes = signal<DrillTypeResponse[]>([]);
	readonly coaches = signal<CoachResponse[]>([]);
	readonly searchQuery = signal('');
	readonly activeDrillTypeId = signal<number | null>(null);
	readonly isCreatingDrill = signal(false);
	readonly isSavingDrill = signal(false);
	readonly isLoading = signal(false);
	readonly loadError = signal<string | null>(null);
	readonly createError = signal<string | null>(null);

	readonly sourceDrills = computed(() => {
		const showSystem = this.isSystemSourceSelected();
		const showUser = this.isUserSourceSelected();

		if (showSystem === showUser) {
			return this.drills();
		}

		const expectedSource: DrillSourceFilter = showSystem ? 'system' : 'user';
		return this.drills().filter(
			(drill) => this.normalizeSource(drill.source) === expectedSource
		);
	});

	readonly totalCount = computed(() => this.sourceDrills().length);

	readonly drillTypeChips = computed<DrillTypeChip[]>(() => {
		const counts = new Map<number, number>();

		for (const drill of this.sourceDrills()) {
			const drillTypeId = drill.drillType?.id;
			if (typeof drillTypeId !== 'number') {
				continue;
			}
			counts.set(drillTypeId, (counts.get(drillTypeId) ?? 0) + 1);
		}

		return this.drillTypes()
			.filter((type) => typeof type.id === 'number')
			.map((type) => ({
				id: type.id as number,
				name: this.formatTypeName(type.name),
				count: counts.get(type.id as number) ?? 0,
			}));
	});

	readonly filteredDrills = computed(() => {
		const activeDrillTypeId = this.activeDrillTypeId();
		const searchQuery = this.searchQuery().trim().toLowerCase();

		return this.sourceDrills().filter((drill) => {
			if (activeDrillTypeId !== null && drill.drillType?.id !== activeDrillTypeId) {
				return false;
			}

			if (!searchQuery) {
				return true;
			}

			const searchableText = [
				drill.name,
				drill.description,
				drill.instructions,
				drill.drillType?.name,
			]
				.filter((value): value is string => Boolean(value))
				.join(' ')
				.toLowerCase();

			return searchableText.includes(searchQuery);
		});
	});

	constructor() {
		effect(() => {
			void this.loadData();
		});
	}

	toggleSystemSource() {
		this.isSystemSourceSelected.update((value) => !value);
	}

	toggleUserSource() {
		this.isUserSourceSelected.update((value) => !value);
	}

	setActiveDrillType(id: number | null) {
		this.activeDrillTypeId.set(id);
	}

	onSearchInput(event: Event) {
		const input = event.target as HTMLInputElement;
		this.searchQuery.set(input.value);
	}

	startCreatingDrill() {
		this.createError.set(null);
		this.isCreatingDrill.set(true);
	}

	cancelCreatingDrill() {
		this.createError.set(null);
		this.isCreatingDrill.set(false);
	}

	async saveDrill(value: DrillFormValue): Promise<void> {
		if (this.isSavingDrill()) {
			return;
		}

		this.createError.set(null);
		this.isSavingDrill.set(true);

		const instructions = value.instructions.trim();
		const payload: CreateDrillRequest = {
			name: value.name.trim(),
			drillTypeId: value.drillTypeId,
			duration: value.duration,
			numberOfPlayers: value.numberOfPlayers,
			coachId: value.coachId,
			instructions: instructions || null,
			// The drill list currently renders description, so mirror instructions for now.
			description: instructions || null,
			demoLink: value.demoLink.trim() || null,
		};

		try {
			await this._api.client.api.drills.post(payload);
			await this.loadData();
			this.isCreatingDrill.set(false);
		} catch {
			this.createError.set('Failed to save drill. Please try again.');
		} finally {
			this.isSavingDrill.set(false);
		}
	}


	private formatTypeName(name?: string | null): string {
		if (!name) {
			return 'Unknown';
		}
		if (name.toLowerCase() === 'base running') {
			return 'Baserunning';
		}
		return name;
	}

	private normalizeSource(source?: string | null): DrillSourceFilter | null {
		if (source === null || source === undefined) {
			return null;
		}

		const normalized = String(source).trim().toLowerCase();
		if (normalized === 'system' || normalized === '0') {
			return 'system';
		}
		if (normalized === 'user' || normalized === '1') {
			return 'user';
		}
		return null;
	}

	private async loadData(): Promise<void> {
		this.isLoading.set(true);
		this.loadError.set(null);

		try {
			const [systemDrills, userDrills, drillTypes, coaches] = await Promise.all([
				this._api.client.api.drills.get({ queryParameters: { source: 0 } }),
				this._api.client.api.drills.get({ queryParameters: { source: 1 } }),
				this._api.client.api.drillTypes.get(),
				this._api.client.api.coaches.byTeam.get(),
			]);

			this.drills.set([...(systemDrills ?? []), ...(userDrills ?? [])]);
			this.drillTypes.set(drillTypes ?? []);
			this.coaches.set(coaches ?? []);
		} catch {
			this.loadError.set('Failed to load drills. Please try again.');
			this.drills.set([]);
			this.drillTypes.set([]);
			this.coaches.set([]);
		} finally {
			this.isLoading.set(false);
		}
	}
}


