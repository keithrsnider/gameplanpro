import { Injectable } from '@angular/core';
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { FetchRequestAdapter, HttpClient } from '@microsoft/kiota-http-fetchlibrary';
import { createApiClient } from './api/apiClient';
import type { ApiClient } from './api/apiClient';

@Injectable({ providedIn: 'root' })
export class ApiClientService {
	readonly client: ApiClient;

	constructor() {
		const credentialFetch = (url: RequestInfo | URL, init?: RequestInit) =>
			fetch(url, { ...init, credentials: 'include' });

		const adapter = new FetchRequestAdapter(
			new AnonymousAuthenticationProvider(),
			undefined,
			undefined,
			new HttpClient(credentialFetch),
		);
		adapter.baseUrl = '';
		this.client = createApiClient(adapter);
	}
}
