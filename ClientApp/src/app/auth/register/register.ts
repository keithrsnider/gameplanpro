import { Component, inject, signal } from '@angular/core';
import type { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import {
	email,
	form,
	FormField,
	minLength,
	required,
	schema,
	submit,
	validate,
} from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import type { FieldTree } from '@angular/forms/signals';
import type { IdentityError } from '../../models/identity-error';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';

interface RegisterFormData {
	displayName: string;
	email: string;
	password: string;
	confirmPassword: string;
}

const registerSchema = schema<RegisterFormData>((f) => {
	required(f.email, { message: 'Email is required.' });
	email(f.email, { message: 'Enter a valid email address.' });

	required(f.password, { message: 'Password is required.' });
	minLength(f.password, 6, { message: 'Must be at least 6 characters.' });

	validate(f.password, ({ value }) =>
		value() && !/[A-Z]/.test(value())
			? { kind: 'noUppercase', message: 'Must contain an uppercase letter.' }
			: undefined
	);
	validate(f.password, ({ value }) =>
		value() && !/[a-z]/.test(value())
			? { kind: 'noLowercase', message: 'Must contain a lowercase letter.' }
			: undefined
	);
	validate(f.password, ({ value }) =>
		value() && !/\d/.test(value())
			? { kind: 'noDigit', message: 'Must contain a digit.' }
			: undefined
	);
	validate(f.password, ({ value }) =>
		value() && !/[^a-zA-Z0-9]/.test(value())
			? { kind: 'noSpecial', message: 'Must contain a special character.' }
			: undefined
	);

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
				await firstValueFrom(
					this._auth.register(email, password, displayName || undefined)
				);
				this._router.navigate(['/']);
			} catch (err) {
				this.apiErrors = this._extractErrors(err as HttpErrorResponse);
			}
			return undefined;
		});
	}

	private _extractErrors(err: HttpErrorResponse): string[] {
		if (Array.isArray(err.error)) {
			return (err.error as IdentityError[]).map((e) => e.description);
		}
		if (err.status === 0) {
			return ['Unable to reach the server. Please check your connection.'];
		}
		return ['An unexpected error occurred. Please try again.'];
	}
}
