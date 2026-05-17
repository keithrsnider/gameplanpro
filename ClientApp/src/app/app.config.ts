import {
	inject,
	provideAppInitializer,
	provideBrowserGlobalErrorListeners,
} from '@angular/core';
import type { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideIcons } from '@ng-icons/core';
import {
	lucideSquareCheckBig,
	lucideLayoutDashboard,
	lucideBookOpen,
	lucideUsers,
	lucideCalendar,
	lucideTrendingUp,
	lucideCircleUser,
	lucidePlus,
	lucideChevronRight,
	lucideArrowLeft,
	lucideClock,
	lucideDownload,
	lucideTrash2,
	lucideArrowUp,
	lucideArrowDown,
	lucideArrowUpDown,
	lucidePlay,
	lucideShield,
	lucideEye,
} from '@ng-icons/lucide';
import { appRoutes } from './app.routes';
import { AuthService } from './auth/auth.service';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideRouter(appRoutes),
		provideHttpClient(withFetch()),
		provideIcons({
			lucideSquareCheckBig,
			lucideLayoutDashboard,
			lucideBookOpen,
			lucideUsers,
			lucideCalendar,
			lucideTrendingUp,
			lucideCircleUser,
			lucidePlus,
			lucideChevronRight,
			lucideArrowLeft,
			lucideDownload,
			lucideClock,
			lucideTrash2,
			lucideArrowUp,
			lucideArrowDown,
			lucideArrowUpDown,
			lucidePlay,
			lucideShield,
			lucideEye
		}),
		provideAppInitializer(() => inject(AuthService).checkAuth()),
	],
};
