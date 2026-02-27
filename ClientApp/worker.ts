interface Env {
	ASSETS: { fetch(request: Request): Promise<Response> };
	API_BASE_URL: string;
}

export default {
	async fetch(request: Request, env: Env): Promise<Response> {
		const url = new URL(request.url);

		if (url.pathname.startsWith('/api/')) {
			const apiUrl = new URL(url.pathname + url.search, env.API_BASE_URL);

			const apiRequest = new Request(apiUrl, {
				method: request.method,
				headers: request.headers,
				body: request.body,
			});

			return fetch(apiRequest);
		}

		return env.ASSETS.fetch(request);
	},
};
