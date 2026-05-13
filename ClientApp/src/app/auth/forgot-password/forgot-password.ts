import { Component, inject, signal } from '@angular/core';
import {
	email,
	form,
	FormField,
	required,
	schema,
	submit,
} from '@angular/forms/signals';
import type { FieldTree } from '@angular/forms/signals';
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';

interface ForgotPasswordFormData {
	email: string;
}

const forgotPasswordSchema = schema<ForgotPasswordFormData>((f) => {
	required(f.email, { message: 'Email is required.' });
	email(f.email, { message: 'Enter a valid email address.' });
});

@Component({
	selector: 'gpp-forgot-password',
	templateUrl: './forgot-password.html',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
	],
})
export class ForgotPasswordComponent {
	private readonly _auth = inject(AuthService);

	readonly model = signal<ForgotPasswordFormData>({ email: '' });
	readonly forgotPasswordForm = form(this.model, forgotPasswordSchema);

	apiErrors: string[] = [];
	successMessage: string | null = null;

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	async onSubmit() {
		this.apiErrors = [];
		this.successMessage = null;

		await submit(this.forgotPasswordForm, async (f) => {
			const { email } = f().value();
			try {
				await this._auth.forgotPassword(email);
				this.successMessage =
					"If an account exists for that email, we've sent a password reset link.";
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
		if (err.responseStatusCode === 429) {
			return ['Too many attempts. Please try again later.'];
		}
		return ['Unable to send a password reset email right now. Please try again.'];
	}
}

