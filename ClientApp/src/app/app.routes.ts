import { Route } from '@angular/router';

export const appRoutes: Route[] = [
	{
		path: 'register',
		loadComponent: () => import('./auth/register/register').then((m) => m.RegisterComponent),
	},
];
