import { Component, input } from '@angular/core';
import type { PlanDrillResponse } from '../../core/api/models';

@Component({
	selector: 'gpp-section-drill-card',
	templateUrl: './section-drill-card.html',
	styleUrl: './section-drill-card.css',
	standalone: true,
})
export class SectionDrillCardComponent {
	readonly drill = input.required<PlanDrillResponse>();
}

