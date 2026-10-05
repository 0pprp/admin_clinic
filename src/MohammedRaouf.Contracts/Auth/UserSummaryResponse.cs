namespace MohammedRaouf.Contracts.Auth;

public sealed class UserSummaryResponse
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? WhatsAppNumber { get; init; }

    public string? Governorate { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required string AccountStatus { get; init; }
}
