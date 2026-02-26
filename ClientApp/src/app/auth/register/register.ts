import { Component, inject } from '@angular/core';
import {
	AbstractControl,
	FormBuilder,
	ReactiveFormsModule,
	ValidationErrors,
	Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import type { IdentityError } from '../../models/identity-error';
import { AuthService } from '../auth.service';

function matchPasswords(control: AbstractControl): ValidationErrors | null {
	const password = control.get('password')?.value;
	const confirm = control.get('confirmPassword')?.value;
	return password === confirm ? null : { passwordMismatch: true };
}

@Component({
	selector: 'gpp-register',
	templateUrl: './register.html',
	imports: [ReactiveFormsModule, ...HlmButtonImports, ...HlmInputImports, ...HlmLabelImports],
})
export class RegisterComponent {
	private readonly _fb = inject(FormBuilder);
	private readonly _auth = inject(AuthService);
	private readonly _router = inject(Router);

	readonly form = this._fb.group(
		{
			displayName: [''],
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(8)]],
			confirmPassword: ['', Validators.required],
		},
		{ validators: matchPasswords },
	);

	apiErrors: IdentityError[] = [];
	submitting = false;

	get email() {
		return this.form.get('email')!;
	}
	get password() {
		return this.form.get('password')!;
	}
	get confirmPassword() {
		return this.form.get('confirmPassword')!;
	}

	onSubmit() {
		if (this.form.invalid) {
			this.form.markAllAsTouched();
			return;
		}

		const { email, password, displayName } = this.form.value;
		this.submitting = true;
		this.apiErrors = [];

		this._auth
			.register(email!, password!, displayName ?? undefined)
			.subscribe({
				next: () => this._router.navigate(['/']),
				error: (err) => {
					this.apiErrors = err?.error ?? [];
					this.submitting = false;
				},
			});
	}
}
