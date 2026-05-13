import type { Route } from '@angular/router';
import { authGuard } from './auth/auth.guard';

export const appRoutes: Route[] = [
	{
		path: 'login',
		loadComponent: () => import('./auth/login/login').then((m) => m.LoginComponent),
	},
	{
		path: 'register',
		loadComponent: () => import('./auth/register/register').then((m) => m.RegisterComponent),
	},
	{
		path: '',
		loadComponent: () => import('./layout/layout').then((m) => m.LayoutComponent),
		canActivate: [authGuard],
		children: [
			{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },
			{
				path: 'dashboard',
				loadComponent: () =>
					import('./dashboard/dashboard').then((m) => m.DashboardComponent),
			},
			{
				path: 'team',
				loadComponent: () =>
					import('./team/team').then((m) => m.TeamComponent),
			},
			{
				path: 'account/reset-password',
				loadComponent: () =>
					import('./auth/reset-password/reset-password').then(
						(m) => m.ResetPasswordComponent
					),
			},
			{
				path: 'practice-plan/:key',
				loadComponent: () =>
					import('./practice-plan/practice-plan-form').then(
						(m) => m.PracticePlanFormComponent
					),
			},
		],
	},
];
