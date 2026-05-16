import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
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
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { FormErrorsComponent } from '../shared/components/form-errors';
import { ApiClientService } from '../core/api-client.service';
import type { CoachResponse, PlayerResponse } from '../core/api/models/index.js';
import { TeamRosterComponent } from './team-roster/team-roster';

interface TeamFormData {
	teamName: string;
	headCoachName: string;
}

const teamSchema = schema<TeamFormData>((f) => {
	required(f.teamName, { message: 'Team name is required.' });
	maxLength(f.teamName, 200, { message: 'Team name must be 200 characters or fewer.' });
	required(f.headCoachName, { message: 'Head Coach name is required.' });
	maxLength(f.headCoachName, 100, {
		message: 'Coach name must be 100 characters or fewer.',
	});
});

@Component({
	selector: 'gpp-team',
	templateUrl: './team.html',
	styleUrl: './team.css',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
		...HlmIconImports,
		TeamRosterComponent,
	],
})
export class TeamComponent {
	private readonly _router = inject(Router);
	private readonly _api = inject(ApiClientService);

	readonly model = signal<TeamFormData>({ teamName: '', headCoachName: '' });
	readonly teamForm = form(this.model, teamSchema);

	readonly assistantCoaches = signal<CoachResponse[]>([]);
	readonly players = signal<PlayerResponse[]>([]);
	headCoachKey?: string;

	hasExistingTeam = true;
	apiErrors: string[] = [];
	loading = signal(false);

	constructor() {
		this.loadTeam();
	}

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	addAssistant() {
		this.assistantCoaches.update((coaches) => [...coaches, { name: '' }]);
	}

	removeAssistant(index: number) {
		this.assistantCoaches.update((coaches) => coaches.filter((_, i) => i !== index));
	}

	updateAssistant(index: number, value: string) {
		this.assistantCoaches.update((coaches) =>
			coaches.map((c, i) => (i === index ? { ...c, name: value } : c))
		);
	}

	async loadTeam() {
		this.loading.set(true);
		try {
			const team = await this._api.client.api.team.get();
			if (team) {
				this.hasExistingTeam = true;
				const headCoach = team.coaches?.find((c) => c.type === 'Head');
				this.headCoachKey = headCoach?.key ?? undefined;
				this.model.set({
					teamName: team.name ?? '',
					headCoachName: headCoach?.name ?? '',
				});
				this.assistantCoaches.set(
					team.coaches?.filter((c) => c.type === 'Assistant') ?? []
				);
				this.players.set(team.players ?? []);
			}
		} catch {
			// 404 = no team yet, which is fine
		} finally {
			this.loading.set(false);
		}
	}

	async onSubmit() {
		this.apiErrors = [];

		await submit(this.teamForm, async (f) => {
			const { teamName, headCoachName } = f().value();
			const coaches = [
				{ key: this.headCoachKey ?? null, name: headCoachName, type: 'Head' },
				...this.assistantCoaches()
					.filter((c) => c.name?.trim())
					.map((c) => ({ key: c.key ?? null, name: c.name, type: 'Assistant' })),
			];

			const players = this.players()
				.filter((p) => p.lastName?.trim())
				.map((p) => ({
					key: p.key ?? null,
					lastName: p.lastName,
					number: p.number ?? 0,
				}));

			const body = { name: teamName, coaches, players };

			try {
				if (this.hasExistingTeam) {
					await this._api.client.api.team.put(body);
				} else {
					await this._api.client.api.team.post(body);
				}
				await this._router.navigate(['/dashboard']);
			} catch {
				this.apiErrors = ['Failed to save team. Please try again.'];
			}
			return undefined;
		});
	}
}
