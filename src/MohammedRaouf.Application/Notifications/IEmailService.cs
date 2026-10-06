namespace MohammedRaouf.Application.Notifications;

public interface IEmailService
{
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default);

    Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);

    Task SendOtpAsync(
        string email,
        string code,
        string purpose,
        CancellationToken cancellationToken = default);
}
