using Microsoft.Extensions.Logging;
using MohammedRaouf.Application.Notifications;

namespace MohammedRaouf.Infrastructure.Email;

public sealed class DisabledEmailService(ILogger<DisabledEmailService> logger) : IEmailService
{
    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        _ = email;
        _ = resetLink;
        logger.LogInformation("Password reset email skipped because the email provider is disabled.");
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default)
    {
        _ = email;
        _ = confirmationLink;
        logger.LogInformation("Email confirmation skipped because the email provider is disabled.");
        return Task.CompletedTask;
    }

    public Task SendOtpAsync(string email, string code, string purpose, CancellationToken cancellationToken = default)
    {
        _ = email;
        _ = code;
        logger.LogInformation("OTP email skipped because the email provider is disabled. Purpose={Purpose}", purpose);
        return Task.CompletedTask;
    }
}
