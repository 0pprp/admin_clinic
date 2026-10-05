using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MohammedRaouf.Application.Notifications;

namespace MohammedRaouf.Infrastructure.Email;

public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        _ = email;
        _ = resetLink;
        logger.LogInformation("Password reset email queued.");
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default)
    {
        _ = email;
        _ = confirmationLink;
        logger.LogInformation("Email confirmation message queued.");
        return Task.CompletedTask;
    }
}
