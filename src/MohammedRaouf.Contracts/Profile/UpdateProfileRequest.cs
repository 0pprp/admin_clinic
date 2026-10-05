namespace MohammedRaouf.Contracts.Profile;

public sealed class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public string Governorate { get; set; } = string.Empty;
}
