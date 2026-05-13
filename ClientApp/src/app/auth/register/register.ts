import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
	apply,
	email,
	form,
	FormField,
	required,
	schema,
	submit,
	validate,
} from '@angular/forms/signals';
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import type { FieldTree } from '@angular/forms/signals';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';
import { passwordStrengthSchema } from '../auth.schemas';

interface RegisterFormData {
	displayName: string;
	email: string;
	password: string;
	confirmPassword: string;
}

const registerSchema = schema<RegisterFormData>((f) => {
	required(f.email, { message: 'Email is required.' });
	email(f.email, { message: 'Enter a valid email address.' });

	apply(f.password, passwordStrengthSchema);

	required(f.confirmPassword, { message: 'Please confirm your password.' });
	validate(f.confirmPassword, ({ value, valueOf }) =>
		value() && value() !== valueOf(f.password)
			? { kind: 'passwordMismatch', message: 'Passwords do not match.' }
			: undefined
	);
});

@Component({
	selector: 'gpp-register',
	templateUrl: './register.html',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
	],
})
export class RegisterComponent {
	private readonly _auth = inject(AuthService);
	private readonly _router = inject(Router);

	readonly model = signal<RegisterFormData>({
		displayName: '',
		email: '',
		password: '',
		confirmPassword: '',
	});

	readonly registrationForm = form(this.model, registerSchema);

	apiErrors: string[] = [];

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	async onSubmit() {
		this.apiErrors = [];

		await submit(this.registrationForm, async (f) => {
			const { email, password, displayName } = f().value();
			try {
				await this._auth.register(email, password, displayName || undefined);
				await this._router.navigate(['/dashboard']);
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
			return ['Registration failed. Please check your details and try again.'];
		}
		return ['An unexpected error occurred. Please try again.'];
	}
}
