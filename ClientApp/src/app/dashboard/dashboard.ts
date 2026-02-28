import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { AuthService } from '../auth/auth.service';

@Component({
	selector: 'gpp-dashboard',
	templateUrl: './dashboard.html',
	imports: [...HlmButtonImports],
})
export class DashboardComponent {
	private readonly _router = inject(Router);
	protected readonly auth = inject(AuthService);

	async signOut() {
		await this.auth.logout();
		await this._router.navigate(['/login']);
	}
}
