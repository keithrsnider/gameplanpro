import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { AuthService } from '../../auth/auth.service';

interface NavItem {
	label: string;
	icon: string;
	route?: string;
}

@Component({
	selector: 'gpp-header',
	templateUrl: './header.html',
	styleUrl: './header.css',
	imports: [...HlmIconImports, RouterLink, RouterLinkActive],
})
export class HeaderComponent {
	private readonly _router = inject(Router);
	private readonly _auth = inject(AuthService);

	readonly navItems: NavItem[] = [
		{ label: 'Dashboard', icon: 'lucideLayoutDashboard', route: '/dashboard' },
		{ label: 'Skills & Drills', icon: 'lucideBookOpen' },
		{ label: 'Team', icon: 'lucideUsers', route: '/team' },
		{ label: 'Schedule', icon: 'lucideCalendar' },
		{ label: 'Analytics', icon: 'lucideTrendingUp' },
	];

	async signOut() {
		await this._auth.logout();
		await this._router.navigate(['/login']);
	}
}
