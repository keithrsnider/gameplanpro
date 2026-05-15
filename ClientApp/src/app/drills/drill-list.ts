import { Component, computed, effect, inject, signal } from '@angular/core';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import type { DrillResponse, DrillTypeResponse } from '../core/api/models/index.js';
import { ApiClientService } from '../core/api-client.service';

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
	imports: [...HlmIconImports, ...HlmInputImports],
})
export class DrillListComponent {
	private readonly _api = inject(ApiClientService);

	readonly source = signal<DrillSourceFilter>('system');
	readonly drills = signal<DrillResponse[]>([]);
	readonly drillTypes = signal<DrillTypeResponse[]>([]);
	readonly searchQuery = signal('');
	readonly activeDrillTypeId = signal<number | null>(null);
	readonly isLoading = signal(false);
	readonly loadError = signal<string | null>(null);

	readonly sourceDrills = computed(() => {
		const expectedSource = this.source() === 'system' ? 'system' : 'user';
		return this.drills().filter(
			(drill) => (drill.source ?? '').toLowerCase() === expectedSource
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
			void this.loadData(this.source());
		});
	}

	setActiveDrillType(id: number | null) {
		this.activeDrillTypeId.set(id);
	}

	onSearchInput(event: Event) {
		const input = event.target as HTMLInputElement;
		this.searchQuery.set(input.value);
	}

	hasVideo(drill: DrillResponse): boolean {
		return Boolean(drill.demoLink?.trim());
	}

	getAccentClass(drillTypeName?: string | null): string {
		const key = this.toTypeKey(drillTypeName);
		return `accent accent-${key}`;
	}

	getBadgeClass(drillTypeName?: string | null): string {
		const key = this.toTypeKey(drillTypeName);
		return `type-badge badge-${key}`;
	}

	displayTypeName(name?: string | null): string {
		return this.formatTypeName(name);
	}

	private toTypeKey(drillTypeName?: string | null): string {
		const normalizedName = (drillTypeName ?? '').trim().toLowerCase();
		switch (normalizedName) {
			case 'warm-up':
			case 'warm up':
				return 'warmup';
			case 'hitting':
				return 'hitting';
			case 'fielding':
				return 'fielding';
			case 'pitching':
				return 'pitching';
			case 'base running':
			case 'baserunning':
				return 'baserunning';
			case 'conditioning':
				return 'conditioning';
			case 'cool-down':
			case 'cool down':
				return 'cooldown';
			default:
				return 'default';
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

	private async loadData(source: DrillSourceFilter): Promise<void> {
		this.isLoading.set(true);
		this.loadError.set(null);

		try {
			const sourceQuery = source === 'system' ? 0 : 1;
			const [drills, drillTypes] = await Promise.all([
				this._api.client.api.drills.get({
					queryParameters: { source: sourceQuery },
				}),
				this._api.client.api.drillTypes.get(),
			]);

			this.drills.set(drills ?? []);
			this.drillTypes.set(drillTypes ?? []);
		} catch {
			this.loadError.set('Failed to load drills. Please try again.');
			this.drills.set([]);
			this.drillTypes.set([]);
		} finally {
			this.isLoading.set(false);
		}
	}
}


