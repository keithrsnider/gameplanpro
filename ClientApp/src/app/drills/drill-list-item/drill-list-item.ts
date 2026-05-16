import { Component, input, signal } from '@angular/core';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import type { DrillResponse } from '../../core/api/models';
import { DrillListItemExpandedComponent } from './drill-list-item-expanded/drill-list-item-expanded';

@Component({
	selector: 'gpp-drill-list-item',
	templateUrl: './drill-list-item.html',
	styleUrl: './drill-list-item.css',
	imports: [...HlmIconImports, DrillListItemExpandedComponent],
})
export class DrillListItemComponent {
	readonly drill = input.required<DrillResponse>();
	readonly isExpanded = signal(false);

	toggleExpanded() {
		this.isExpanded.update((value) => !value);
	}

	hasVideo(): boolean {
		return Boolean(this.drill().demoLink?.trim());
	}

	getAccentClass(): string {
		const key = this.toTypeKey(this.drill().drillType?.name);
		return `accent accent-${key}`;
	}

	getBadgeClass(): string {
		const key = this.toTypeKey(this.drill().drillType?.name);
		return `type-badge badge-${key}`;
	}

	displayTypeName(): string {
		return this.formatTypeName(this.drill().drillType?.name);
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
}


