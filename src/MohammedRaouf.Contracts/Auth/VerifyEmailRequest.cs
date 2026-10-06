namespace MohammedRaouf.Contracts.Auth;

public sealed class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// EmailVerification | PasswordReset | GoogleLogin
    /// </summary>
    public string Purpose { get; set; } = "EmailVerification";

    public bool RememberMe { get; set; }
}
