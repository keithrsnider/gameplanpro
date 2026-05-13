import { minLength, required, schema, validate } from '@angular/forms/signals';

/**
 * Reusable schema for password strength validation.
 * Apply to any `string` field with `apply(f.password, passwordStrengthSchema)`.
 */
export const passwordStrengthSchema = schema<string>((f) => {
	required(f, { message: 'Password is required.' });
	minLength(f, 6, { message: 'Must be at least 6 characters.' });

	validate(f, ({ value }) =>
		value() && !/[A-Z]/.test(value())
			? { kind: 'noUppercase', message: 'Must contain an uppercase letter.' }
			: undefined
	);
	validate(f, ({ value }) =>
		value() && !/[a-z]/.test(value())
			? { kind: 'noLowercase', message: 'Must contain a lowercase letter.' }
			: undefined
	);
	validate(f, ({ value }) =>
		value() && !/\d/.test(value())
			? { kind: 'noDigit', message: 'Must contain a digit.' }
			: undefined
	);
	validate(f, ({ value }) =>
		value() && !/[^a-zA-Z0-9]/.test(value())
			? { kind: 'noSpecial', message: 'Must contain a special character.' }
			: undefined
	);
});
