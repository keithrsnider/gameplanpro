namespace Api.Extensions;

public static class CorsServiceExtensions
{
    public const string PolicyName = "AllowFrontend";

    public static IServiceCollection AddCorsServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
