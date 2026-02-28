import type { Route } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const appRoutes: Route[] = [
	{
		path: '',
		redirectTo: 'dashboard',
		pathMatch: 'full',
	},
	{
		path: 'login',
		loadComponent: () => import('./auth/login/login').then((m) => m.LoginComponent),
	},
	{
		path: 'register',
		loadComponent: () => import('./auth/register/register').then((m) => m.RegisterComponent),
	},
	{
		path: 'dashboard',
		loadComponent: () => import('./dashboard/dashboard').then((m) => m.DashboardComponent),
		canActivate: [authGuard],
	},
];
