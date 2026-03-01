import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { AuthService } from '../../auth/auth.service';

interface NavItem {
	label: string;
	icon: string;
	active: boolean;
}

@Component({
	selector: 'gpp-header',
	templateUrl: './header.html',
	styleUrl: './header.css',
	imports: [...HlmIconImports],
})
export class HeaderComponent {
	private readonly _router = inject(Router);
	private readonly _auth = inject(AuthService);

	readonly navItems: NavItem[] = [
		{ label: 'Dashboard', icon: 'lucideLayoutDashboard', active: true },
		{ label: 'Skills & Drills', icon: 'lucideBookOpen', active: false },
		{ label: 'Team', icon: 'lucideUsers', active: false },
		{ label: 'Schedule', icon: 'lucideCalendar', active: false },
		{ label: 'Analytics', icon: 'lucideTrendingUp', active: false },
	];

	async signOut() {
		await this._auth.logout();
		await this._router.navigate(['/login']);
	}
}
