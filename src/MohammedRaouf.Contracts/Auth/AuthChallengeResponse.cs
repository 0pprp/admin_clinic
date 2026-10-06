namespace MohammedRaouf.Contracts.Auth;

public sealed class AuthChallengeResponse
{
    public required bool RequiresEmailVerification { get; init; }

    public required string Email { get; init; }

    public required string Message { get; init; }

    public string Purpose { get; init; } = "EmailVerification";
}
