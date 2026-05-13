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
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';
import { passwordStrengthSchema } from '../auth.schemas';

interface ResetPasswordFormData {
	currentPassword: string;
	newPassword: string;
	confirmPassword: string;
}

const resetPasswordSchema = schema<ResetPasswordFormData>((f) => {
	required(f.currentPassword, { message: 'Current password is required.' });

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
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
	],
})
export class ResetPasswordComponent {
	private readonly _auth = inject(AuthService);

	readonly model = signal<ResetPasswordFormData>({
		currentPassword: '',
		newPassword: '',
		confirmPassword: '',
	});
	readonly resetPasswordForm = form(this.model, resetPasswordSchema);

	apiErrors: string[] = [];
	successMessage: string | null = null;

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	async onSubmit() {
		this.apiErrors = [];
		this.successMessage = null;

		await submit(this.resetPasswordForm, async (f) => {
			const { currentPassword, newPassword } = f().value();
			try {
				await this._auth.resetPassword(currentPassword, newPassword);
				this.successMessage = 'Your password has been updated.';
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
			return ['Unable to update password. Check your current password and try again.'];
		}
		if (err.responseStatusCode === 401) {
			return ['Your session has expired. Please sign in again.'];
		}
		return ['An unexpected error occurred. Please try again.'];
	}
}


