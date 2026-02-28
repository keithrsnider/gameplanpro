import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
	email,
	form,
	FormField,
	required,
	schema,
	submit,
} from '@angular/forms/signals';
import { signal } from '@angular/core';
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import type { FieldTree } from '@angular/forms/signals';
import { AuthService } from '../auth.service';
import { FormErrorsComponent } from '../../shared/components/form-errors';

interface LoginFormData {
	email: string;
	password: string;
}

const loginSchema = schema<LoginFormData>((f) => {
	required(f.email, { message: 'Email is required.' });
	email(f.email, { message: 'Enter a valid email address.' });

	required(f.password, { message: 'Password is required.' });
});

@Component({
	selector: 'gpp-login',
	templateUrl: './login.html',
	imports: [
		RouterLink,
		FormField,
		FormErrorsComponent,
		...HlmButtonImports,
		...HlmInputImports,
		...HlmLabelImports,
	],
})
export class LoginComponent {
	private readonly _auth = inject(AuthService);
	private readonly _router = inject(Router);
	private readonly _route = inject(ActivatedRoute);

	readonly model = signal<LoginFormData>({ email: '', password: '' });
	readonly loginForm = form(this.model, loginSchema);

	apiErrors: string[] = [];

	fieldHasError(field: FieldTree<unknown>): true | undefined {
		return field().touched() && !field().valid() ? true : undefined;
	}

	async onSubmit() {
		this.apiErrors = [];

		await submit(this.loginForm, async (f) => {
			const { email, password } = f().value();
			try {
				await this._auth.login(email, password);
				const returnUrl = this._route.snapshot.queryParams['returnUrl'] as string | undefined;
				await this._router.navigateByUrl(returnUrl ?? '/dashboard');
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
		return ['Invalid email or password.'];
	}
}
