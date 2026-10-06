namespace MohammedRaouf.Contracts.Auth;

public sealed class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Six-digit OTP from email, or legacy encoded Identity reset token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}
