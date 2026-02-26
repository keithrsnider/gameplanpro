import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';

export interface AuthUserResponse {
	id: string;
	email: string;
	displayName: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
	private readonly _http = inject(HttpClient);

	readonly currentUser = signal<AuthUserResponse | null>(null);

	register(email: string, password: string, displayName?: string) {
		return this._http
			.post<AuthUserResponse>(
				'/api/auth/register',
				{ email, password, displayName },
				{ withCredentials: true },
			)
			.pipe(tap((user) => this.currentUser.set(user)));
	}
}
