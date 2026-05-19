import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
	form,
	FormField,
	maxLength,
	required,
	schema,
	submit,
} from '@angular/forms/signals';
import type { FieldTree } from '@angular/forms/signals';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { BrnSelect, BrnSelectImports } from '@spartan-ng/brain/select';
import { FormErrorsComponent } from '../shared/components/form-errors';
import { HorizontalRuleComponent } from '../shared/components/horizontal-rule';
import { ApiClientService } from '../core/api-client.service';
import type { EditableSection } from './practice-plan-form.types';
import { SectionEditorComponent } from './section-editor/section-editor';

interface PracticePlanFormData {
	name: string;
	description: string;
}

const practicePlanSchema = schema<PracticePlanFormData>((f) => {
	required(f.name, { message: 'Plan name is required.' });
	maxLength(f.name, 200, { message: 'Name must be 200 characters or fewer.' });
	maxLength(f.description, 2000, {
		message: 'Description must be 2000 characters or fewer.',
	});
});

const DURATION_OPTIONS = [30, 45, 60, 75, 90, 105, 120];

@Component({
	selector: 'gpp-practice-plan-form',
	templateUrl: './practice-plan-form.html',
	styleUrl: './practice-plan-form.css',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		HorizontalRuleComponent,
		SectionEditorComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
		...HlmSelectImports,
		...HlmTextareaImports,
		...HlmIconImports,
		...BrnSelectImports,
		BrnSelect,
	],
})
export class PracticePlanFormComponent {
	private readonly _router = inject(Router);
	private readonly _route = inject(ActivatedRoute);
	private readonly _api = inject(ApiClientService);

	readonly isEditMode: boolean;
	readonly planKey: string | null;
	readonly durations = DURATION_OPTIONS;

	readonly model = signal<PracticePlanFormData>({ name: '', description: '' });
	readonly planForm = form(this.model, practicePlanSchema);

	readonly selectedLocation = signal<string>('');
	readonly selectedDuration = signal<string>('');
	readonly planSaveMessage = signal<string | null>(null);
	readonly initialSections = signal<EditableSection[]>([]);

	apiErrors: string[] = [];
	loading = signal(false);

	constructor() {
		const key = this._route.snapshot.paramMap.get('key');
		this.isEditMode = key !== null && key !== 'new';
		this.planKey = this.isEditMode ? key : null;

		if (this.isEditMode && this.planKey) {
			void this.loadPlan(this.planKey);
		}
	}

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	updateSelectedLocation(value: string | string[] | undefined) {
		this.selectedLocation.set(typeof value === 'string' ? value : '');
	}

	updateSelectedDuration(value: string | string[] | undefined) {
		this.selectedDuration.set(typeof value === 'string' ? value : '');
	}

	async loadPlan(key: string) {
		this.loading.set(true);
		try {
			const plan = await this._api.client.api.practicePlans.byKeyId(key).get();
			if (!plan) {
				this.apiErrors = ['Failed to load practice plan.'];
				return;
			}

			this.model.set({
				name: plan.name ?? '',
				description: plan.description ?? '',
			});
			if (plan.location) this.selectedLocation.set(plan.location);
			if (plan.intendedDuration) {
				this.selectedDuration.set(String(plan.intendedDuration));
			}
			if (plan.sections?.length) {
				this.initialSections.set(
					plan.sections.map((section, index) => ({
						key: String(section.key ?? ''),
						name: section.name ?? '',
						note: section.note ?? '',
						displayOrder: section.displayOrder ?? index + 1,
						planDrills: [...(section.planDrills ?? [])],
						saveState: 'idle',
						errorMessage: null,
					}))
				);
			}
		} catch {
			this.apiErrors = ['Failed to load practice plan.'];
		} finally {
			this.loading.set(false);
		}
	}

	async onSubmit() {
		this.apiErrors = [];
		this.planSaveMessage.set(null);

		await submit(this.planForm, async (f) => {
			const { name, description } = f().value();
			const body = {
				name,
				location: this.selectedLocation() || null,
				intendedDuration: this.selectedDuration() ? Number(this.selectedDuration()) : null,
				description: description || null,
			};

			try {
				if (this.isEditMode && this.planKey) {
					await this._api.client.api.practicePlans.byKeyId(this.planKey).put(body);
					this.planSaveMessage.set('Practice plan details saved.');
				} else {
					const created = await this._api.client.api.practicePlans.post(body);
					if (created?.key) {
						await this._router.navigate(['/practice-plan', created.key]);
						return undefined;
					}

					await this._router.navigate(['/dashboard']);
					return undefined;
				}
			} catch {
				this.apiErrors = [
					this.isEditMode
						? 'Failed to save practice plan. Please try again.'
						: 'Failed to create practice plan. Please try again.',
				];
			}
			return undefined;
		});
	}
}
