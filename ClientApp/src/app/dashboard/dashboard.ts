import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { PracticePlanListComponent } from './practice-plan-list/practice-plan-list';

@Component({
	selector: 'gpp-dashboard',
	templateUrl: './dashboard.html',
	styleUrl: './dashboard.css',
	imports: [RouterLink, ...HlmButtonImports, ...HlmIconImports, PracticePlanListComponent],
})
export class DashboardComponent {}
