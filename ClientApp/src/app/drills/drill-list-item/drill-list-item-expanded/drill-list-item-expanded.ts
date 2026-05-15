import { Component, input } from '@angular/core';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import type { DrillResponse } from '../../../core/api/models';

@Component({
	selector: 'gpp-drill-list-item-expanded',
	templateUrl: './drill-list-item-expanded.html',
	styleUrl: './drill-list-item-expanded.css',
	imports: [...HlmIconImports],
})
export class DrillListItemExpandedComponent {
	readonly drill = input.required<DrillResponse>();

	hasVideo(): boolean {
		return Boolean(this.drill().demoLink?.trim());
	}

	instructionsText(): string {
		return this.drill().instructions?.trim() || 'No instructions provided.';
	}

	playersText(): string {
		const players = this.drill().numberOfPlayers;
		return typeof players === 'number' ? `${players}` : 'N/A';
	}

	coachText(): string {
		return this.drill().coach?.name?.trim() || 'Unassigned';
	}
}

