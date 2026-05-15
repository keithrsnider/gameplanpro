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
		path: 'forgot-password',
		loadComponent: () =>
			import('./auth/forgot-password/forgot-password').then(
				(m) => m.ForgotPasswordComponent
			),
	},
	{
		path: 'reset-password',
		loadComponent: () =>
			import('./auth/reset-password/reset-password').then((m) => m.ResetPasswordComponent),
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
				path: 'drills',
				loadComponent: () => import('./drills/drill-list').then((m) => m.DrillListComponent),
			},
			{
				path: 'team',
				loadComponent: () =>
					import('./team/team').then((m) => m.TeamComponent),
			},
			{
				path: 'account/change-password',
				loadComponent: () =>
					import('./auth/change-password/change-password').then(
						(m) => m.ChangePasswordComponent
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
