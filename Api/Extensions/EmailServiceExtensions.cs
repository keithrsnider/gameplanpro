using Api.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Resend;

namespace Api.Extensions;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = config["Resend:ApiKey"]!;
        });
        services.AddTransient<IResend, ResendClient>();
        services.AddTransient<IEmailSender, EmailSender>();

        return services;
    }
}
