import { Component, inject, signal } from '@angular/core';
import type { PracticePlanListResponse } from '../../core/api/models/index.js';
import { ApiClientService } from '../../core/api-client.service';
import { PracticePlanListItemComponent } from './practice-plan-list-item/practice-plan-list-item';

@Component({
	selector: 'gpp-practice-plan-list',
	templateUrl: './practice-plan-list.html',
	styleUrl: './practice-plan-list.css',
	imports: [PracticePlanListItemComponent],
})
export class PracticePlanListComponent {
	private readonly _api = inject(ApiClientService);

	plans = signal<PracticePlanListResponse[]>([]);

	constructor() {
		this._loadPlans();
	}

	private async _loadPlans(): Promise<void> {
		var plans = await this._api.client.api.practicePlans.get();
		this.plans.set(plans ?? []);
	}
}
