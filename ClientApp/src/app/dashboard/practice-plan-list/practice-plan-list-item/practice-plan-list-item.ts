import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import type { PracticePlanItem } from '../../dashboard';

@Component({
	selector: 'gpp-practice-plan-list-item',
	templateUrl: './practice-plan-list-item.html',
	styleUrl: './practice-plan-list-item.css',
	imports: [RouterLink, ...HlmIconImports],
})
export class PracticePlanListItemComponent {
	plan = input.required<PracticePlanItem>();
}
