namespace MohammedRaouf.Application.Security;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int LoginPermitLimit { get; set; } = 5;

    public int LoginWindowSeconds { get; set; } = 60;

    public int RegisterPermitLimit { get; set; } = 3;

    public int RegisterWindowSeconds { get; set; } = 600;

    public int ForgotPasswordPermitLimit { get; set; } = 3;

    public int ForgotPasswordWindowSeconds { get; set; } = 900;

    public int ResetPasswordPermitLimit { get; set; } = 5;

    public int ResetPasswordWindowSeconds { get; set; } = 600;

    public int RefreshPermitLimit { get; set; } = 30;

    public int RefreshWindowSeconds { get; set; } = 60;

    public int ActivationRedeemPermitLimit { get; set; } = 5;

    public int ActivationRedeemWindowSeconds { get; set; } = 60;

    public int ProgressPermitLimit { get; set; } = 120;

    public int ProgressWindowSeconds { get; set; } = 60;

    public int ConsultationPermitLimit { get; set; } = 3;

    public int ConsultationWindowSeconds { get; set; } = 900;

    public int ContactPermitLimit { get; set; } = 5;

    public int ContactWindowSeconds { get; set; } = 600;
}
