namespace Api.Extensions;

public static class GoogleAuthServiceExtensions
{
    public static IServiceCollection AddGoogleAuth(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = config["Authentication:Google:ClientId"]!;
                options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
            });

        return services;
    }
}
