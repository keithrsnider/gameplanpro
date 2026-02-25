using Api.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Resend;

namespace Api.Services;

public class EmailSender(IResend resend, IConfiguration config) : IEmailSender
{
    private readonly string _from = config["Resend:From"]
        ?? "onboarding@resend.dev";

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new EmailMessage
        {
            From = _from,
            Subject = subject,
            HtmlBody = htmlMessage,
        };
        message.To.Add(email);

        await resend.EmailSendAsync(message);
    }
}
