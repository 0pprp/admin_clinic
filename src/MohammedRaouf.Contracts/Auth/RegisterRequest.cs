namespace MohammedRaouf.Contracts.Auth;

public sealed class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public string Governorate { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PasswordConfirmation { get; set; } = string.Empty;

    public bool TermsAccepted { get; set; }
}
