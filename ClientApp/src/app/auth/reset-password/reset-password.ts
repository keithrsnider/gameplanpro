import { Component, inject, signal } from '@angular/core';
import {
	apply,
	form,
	FormField,
	required,
	schema,
	submit,
	validate,
} from '@angular/forms/signals';
import type { FieldTree } from '@angular/forms/signals';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';
import { passwordStrengthSchema } from '../auth.schemas';

interface ResetPasswordFormData {
	newPassword: string;
	confirmPassword: string;
}

const resetPasswordSchema = schema<ResetPasswordFormData>((f) => {
	apply(f.newPassword, passwordStrengthSchema);

	required(f.confirmPassword, { message: 'Please confirm your new password.' });
	validate(f.confirmPassword, ({ value, valueOf }) =>
		value() && value() !== valueOf(f.newPassword)
			? { kind: 'passwordMismatch', message: 'Passwords do not match.' }
			: undefined
	);
});

@Component({
	selector: 'gpp-reset-password',
	templateUrl: './reset-password.html',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
	],
})
export class ResetPasswordComponent {
	private readonly _auth = inject(AuthService);
	private readonly _route = inject(ActivatedRoute);
	private readonly _router = inject(Router);

	readonly email = this._route.snapshot.queryParamMap.get('email') ?? '';
	readonly token = this._route.snapshot.queryParamMap.get('token') ?? '';
	readonly hasValidLink = this.email.length > 0 && this.token.length > 0;
	readonly model = signal<ResetPasswordFormData>({
		newPassword: '',
		confirmPassword: '',
	});
	readonly resetPasswordForm = form(this.model, resetPasswordSchema);

	apiErrors: string[] = this.hasValidLink ? [] : ['This reset link is invalid or incomplete.'];

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	async onSubmit() {
		this.apiErrors = this.hasValidLink ? [] : ['This reset link is invalid or incomplete.'];

		if (!this.hasValidLink) {
			return;
		}

		await submit(this.resetPasswordForm, async (f) => {
			const { newPassword } = f().value();
			try {
				await this._auth.completePasswordReset(this.email, this.token, newPassword);
				await this._router.navigate(['/login'], {
					queryParams: { passwordReset: 'success' },
				});
			} catch (err) {
				this.apiErrors = this._extractErrors(err as DefaultApiError);
			}
			return undefined;
		});
	}

	private _extractErrors(err: DefaultApiError): string[] {
		if (err.responseStatusCode === 0 || err.responseStatusCode === undefined) {
			return ['Unable to reach the server. Please check your connection.'];
		}
		if (err.responseStatusCode === 400) {
			return [
				'This reset link is invalid or has expired. Request a new password reset email and try again.',
			];
		}
		if (err.responseStatusCode === 429) {
			return ['Too many attempts. Please try again later.'];
		}
		return ['An unexpected error occurred. Please try again.'];
	}
}


