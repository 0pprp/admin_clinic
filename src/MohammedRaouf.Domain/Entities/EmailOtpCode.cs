using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class EmailOtpCode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ConsumedAt { get; set; }

    public int AttemptCount { get; set; }

    public ApplicationUser? User { get; set; }
}

public static class EmailOtpPurposes
{
    public const string EmailVerification = "EmailVerification";

    public const string PasswordReset = "PasswordReset";

    public const string GoogleLogin = "GoogleLogin";
}
