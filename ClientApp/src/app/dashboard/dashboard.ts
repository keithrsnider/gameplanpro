import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { PracticePlanListComponent } from './practice-plan-list/practice-plan-list';

export interface PracticePlanItem {
	key: string;
	name: string;
	intendedDuration: number | null;
	description: string | null;
}

@Component({
	selector: 'gpp-dashboard',
	templateUrl: './dashboard.html',
	styleUrl: './dashboard.css',
	imports: [RouterLink, ...HlmButtonImports, ...HlmIconImports, PracticePlanListComponent],
})
export class DashboardComponent {
	practicePlans: PracticePlanItem[] = [
		{
			key: '1a2b3c4d-0001-0000-0000-000000000001',
			name: 'Tuesday Hitting Focus',
			intendedDuration: 90,
			description: 'Focus on swing mechanics and live at-bats with pitching machine.',
		},
		{
			key: '1a2b3c4d-0002-0000-0000-000000000002',
			name: 'Pre-Game Warm-Up',
			intendedDuration: 30,
			description: null,
		},
		{
			key: '1a2b3c4d-0003-0000-0000-000000000003',
			name: 'Weekend Full Practice',
			intendedDuration: 120,
			description:
				'Full team practice covering fielding, hitting stations, and base running drills.',
		},
	];
}
