using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions;

public static class RateLimitingServiceExtensions
{
	public const string AuthPolicy = "auth";

	public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			options.AddPolicy(AuthPolicy, context =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
					factory: _ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = 10,
						Window = TimeSpan.FromMinutes(1),
					}));
		});

		return services;
	}
}
