import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { Component, input, output } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import type { EditableSection } from '../practice-plan-form.types';
import { SectionDrillCardComponent } from '../section-drill-card/section-drill-card';

export interface SectionInputChange {
	sectionKey: string;
	value: string;
}

@Component({
	selector: 'gpp-section-card',
	templateUrl: './section-card.html',
	styleUrl: './section-card.css',
	imports: [
		CdkDragHandle,
		SectionDrillCardComponent,
		...HlmButtonImports,
		...HlmIconImports,
		...HlmInputImports,
		...HlmLabelImports,
		...HlmTextareaImports,
	],
	standalone: true,
})
export class SectionCardComponent {
	readonly section = input.required<EditableSection>();
	readonly drillCount = input.required<number>();
	readonly durationMinutes = input.required<number>();

	readonly sectionNameInput = output<SectionInputChange>();
	readonly sectionNoteInput = output<SectionInputChange>();
	readonly deleteRequested = output<string>();
	readonly retrySaveRequested = output<string>();

	onSectionNameInput(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.sectionNameInput.emit({ sectionKey: this.section().key, value });
	}

	onSectionNoteInput(event: Event) {
		const value = (event.target as HTMLTextAreaElement).value;
		this.sectionNoteInput.emit({ sectionKey: this.section().key, value });
	}

	requestDelete() {
		this.deleteRequested.emit(this.section().key);
	}

	requestRetrySave() {
		this.retrySaveRequested.emit(this.section().key);
	}
}

