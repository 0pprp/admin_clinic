using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Application.Auth;

public interface IEmailOtpService
{
    Task<string> IssueAsync(ApplicationUser user, string purpose, CancellationToken cancellationToken = default);

    Task<bool> VerifyAsync(
        ApplicationUser user,
        string purpose,
        string code,
        CancellationToken cancellationToken = default);
}
