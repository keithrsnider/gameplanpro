interface Env {
	ASSETS: { fetch(request: Request): Promise<Response> };
	API_BASE_URL: string;
}

export default {
	async fetch(request: Request, env: Env): Promise<Response> {
		const url = new URL(request.url);

		if (url.pathname.startsWith('/api/')) {
			try {
				if (!env.API_BASE_URL) {
					return new Response('API_BASE_URL not configured', { status: 502 });
				}

				const apiUrl = new URL(url.pathname + url.search, env.API_BASE_URL);

				const headers = new Headers(request.headers);
				headers.set('Host', new URL(env.API_BASE_URL).host);

				const apiRequest = new Request(apiUrl, {
					method: request.method,
					headers,
					body: request.body,
				});

				return fetch(apiRequest);
			} catch (e) {
				return new Response(
					`Worker proxy error: ${e instanceof Error ? e.message : String(e)}`,
					{ status: 502 },
				);
			}
		}

		return env.ASSETS.fetch(request);
	},
};
