import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import type { CoachResponse, DrillTypeResponse } from '../../core/api/models/index.js';

export interface DrillFormValue {
	name: string;
	drillTypeId: number | null;
	duration: number;
	numberOfPlayers: number | null;
	coachId: number | null;
	instructions: string;
	demoLink: string;
}

const DEFAULT_FORM_VALUE: DrillFormValue = {
	name: '',
	drillTypeId: null,
	duration: 10,
	numberOfPlayers: null,
	coachId: null,
	instructions: '',
	demoLink: '',
};

@Component({
	selector: 'gpp-drill-form',
	templateUrl: './drill-form.html',
	styleUrl: './drill-form.css',
	imports: [...HlmInputImports, ...HlmTextareaImports],
})
export class DrillFormComponent {
	@Input({ required: true }) drillTypes: DrillTypeResponse[] = [];
	@Input() coaches: CoachResponse[] = [];
	@Input() submitLabel = 'Save';
	@Input() isSubmitting = false;

	@Output() readonly save = new EventEmitter<DrillFormValue>();
	@Output() readonly cancelRequested = new EventEmitter<void>();

	readonly form = signal<DrillFormValue>({ ...DEFAULT_FORM_VALUE });
	readonly canSubmit = computed(() => {
		const value = this.form();
		return value.name.trim().length > 0 && value.drillTypeId !== null && value.duration > 0;
	});

	onNameInput(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.form.update((current) => ({ ...current, name: value }));
	}

	onDrillTypeChange(event: Event) {
		const value = (event.target as HTMLSelectElement).value;
		const drillTypeId = value ? Number(value) : null;
		this.form.update((current) => ({ ...current, drillTypeId }));
	}

	onDurationInput(event: Event) {
		const raw = (event.target as HTMLInputElement).value;
		const nextDuration = Number(raw);
		this.form.update((current) => ({
			...current,
			duration: Number.isFinite(nextDuration) && nextDuration > 0 ? nextDuration : 1,
		}));
	}

	decrementDuration() {
		this.form.update((current) => ({
			...current,
			duration: Math.max(1, current.duration - 1),
		}));
	}

	incrementDuration() {
		this.form.update((current) => ({ ...current, duration: current.duration + 1 }));
	}

	onPlayerCountInput(event: Event) {
		const raw = (event.target as HTMLInputElement).value;
		const value = raw === '' ? null : Number(raw);
		this.form.update((current) => ({
			...current,
			numberOfPlayers: value !== null && Number.isFinite(value) && value > 0 ? value : null,
		}));
	}

	onCoachChange(event: Event) {
		const value = (event.target as HTMLSelectElement).value;
		const coachId = value ? Number(value) : null;
		this.form.update((current) => ({ ...current, coachId }));
	}

	onInstructionsInput(event: Event) {
		const value = (event.target as HTMLTextAreaElement).value;
		this.form.update((current) => ({ ...current, instructions: value }));
	}

	onDemoLinkInput(event: Event) {
		const value = (event.target as HTMLInputElement).value;
		this.form.update((current) => ({ ...current, demoLink: value }));
	}

	onSubmit() {
		if (!this.canSubmit() || this.isSubmitting) {
			return;
		}

		const value = this.form();
		this.save.emit({
			name: value.name.trim(),
			drillTypeId: value.drillTypeId,
			duration: value.duration,
			numberOfPlayers: value.numberOfPlayers,
			coachId: value.coachId,
			instructions: value.instructions.trim(),
			demoLink: value.demoLink.trim(),
		});
	}

	onCancel() {
		this.form.set({ ...DEFAULT_FORM_VALUE });
		this.cancelRequested.emit();
	}
}

