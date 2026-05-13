import { Component, inject, signal } from '@angular/core';
import {
	form,
	FormField,
	minLength,
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

interface ResetPasswordFormData {
	currentPassword: string;
	newPassword: string;
	confirmPassword: string;
}

const resetPasswordSchema = schema<ResetPasswordFormData>((f) => {
	required(f.currentPassword, { message: 'Current password is required.' });

	required(f.newPassword, { message: 'New password is required.' });
	minLength(f.newPassword, 6, { message: 'Must be at least 6 characters.' });
	validate(f.newPassword, ({ value }) =>
		value() && !/[A-Z]/.test(value())
			? { kind: 'noUppercase', message: 'Must contain an uppercase letter.' }
			: undefined
	);
	validate(f.newPassword, ({ value }) =>
		value() && !/[a-z]/.test(value())
			? { kind: 'noLowercase', message: 'Must contain a lowercase letter.' }
			: undefined
	);
	validate(f.newPassword, ({ value }) =>
		value() && !/\d/.test(value())
			? { kind: 'noDigit', message: 'Must contain a digit.' }
			: undefined
	);
	validate(f.newPassword, ({ value }) =>
		value() && !/[^a-zA-Z0-9]/.test(value())
			? { kind: 'noSpecial', message: 'Must contain a special character.' }
			: undefined
	);

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


