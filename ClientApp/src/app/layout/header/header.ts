import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
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
	private readonly _elementRef = inject(ElementRef<HTMLElement>);
	private readonly _router = inject(Router);
	private readonly _auth = inject(AuthService);

	readonly navItems: NavItem[] = [
		{ label: 'My Practice Plans', icon: 'lucideLayoutDashboard', route: '/dashboard' },
		{ label: 'Skills & Drills', icon: 'lucideBookOpen', route: '/drills' },
		{ label: 'Team', icon: 'lucideUsers', route: '/team' },
		// { label: 'Schedule', icon: 'lucideCalendar' },
		// { label: 'Analytics', icon: 'lucideTrendingUp' },
	];
	readonly currentUser = this._auth.currentUser;
	readonly userMenuOpen = signal(false);

	toggleUserMenu() {
		this.userMenuOpen.update((isOpen) => !isOpen);
	}

	closeUserMenu() {
		this.userMenuOpen.set(false);
	}

	async goToChangePassword() {
		this.closeUserMenu();
		await this._router.navigate(['/account/change-password']);
	}

	async signOut() {
		this.closeUserMenu();
		await this._auth.logout();
		await this._router.navigate(['/login']);
	}

	@HostListener('document:click', ['$event'])
	onDocumentClick(event: MouseEvent) {
		if (!this._elementRef.nativeElement.contains(event.target as Node)) {
			this.closeUserMenu();
		}
	}

	@HostListener('document:keydown.escape')
	onEscapeKey() {
		this.closeUserMenu();
	}
}
