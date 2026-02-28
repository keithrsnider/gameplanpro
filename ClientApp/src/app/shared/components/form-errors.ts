import { Component, input } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';

@Component({
	selector: 'gpp-form-errors',
	template: `
		@if (field()().touched()) {
			@for (error of field()().errors(); track error.kind) {
				<p class="text-sm text-destructive">{{ error.message }}</p>
			}
		}
	`,
})
export class FormErrorsComponent {
	readonly field = input.required<FieldTree<unknown>>();
}
