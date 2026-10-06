namespace MohammedRaouf.Contracts.Auth;

public sealed class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
