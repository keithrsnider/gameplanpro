import { Injectable, inject, signal } from '@angular/core';
import type { DefaultApiError } from '@microsoft/kiota-abstractions';
import type { AuthUserResponse } from '../core/api/models/index.js';
import { ApiClientService } from '../core/api-client.service';

export type { AuthUserResponse };

@Injectable({ providedIn: 'root' })
export class AuthService {
	private readonly _api = inject(ApiClientService);

	readonly currentUser = signal<AuthUserResponse | null>(null);

	async register(email: string, password: string, displayName?: string): Promise<AuthUserResponse> {
		const user = await this._api.client.api.auth.register.post({ email, password, displayName });
		if (!user) throw new Error('Unexpected empty response from register');
		this.currentUser.set(user);
		return user;
	}

	async login(email: string, password: string): Promise<AuthUserResponse> {
		const user = await this._api.client.api.auth.login.post({ email, password });
		if (!user) throw new Error('Unexpected empty response from login');
		this.currentUser.set(user);
		return user;
	}

	async logout(): Promise<void> {
		await this._api.client.api.auth.logout.post();
		this.currentUser.set(null);
	}

	async checkAuth(): Promise<void> {
		try {
			const user = await this._api.client.api.auth.me.get();
			this.currentUser.set(user ?? null);
		} catch (err) {
			const apiErr = err as DefaultApiError;
			if (apiErr.responseStatusCode === 401) {
				this.currentUser.set(null);
			}
		}
	}
}
