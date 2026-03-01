import { Component, input } from '@angular/core';
import { PracticePlanListItemComponent } from './practice-plan-list-item/practice-plan-list-item';
import type { PracticePlanItem } from '../dashboard';

@Component({
	selector: 'gpp-practice-plan-list',
	templateUrl: './practice-plan-list.html',
	styleUrl: './practice-plan-list.css',
	imports: [PracticePlanListItemComponent],
})
export class PracticePlanListComponent {
	plans = input.required<PracticePlanItem[]>();
}
