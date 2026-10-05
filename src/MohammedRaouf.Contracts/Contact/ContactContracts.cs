namespace MohammedRaouf.Contracts.Contact;

public sealed class CreateContactMessageRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Website { get; set; }
}

public sealed class CreateContactMessageResponse
{
    public required Guid Id { get; init; }

    public required string Status { get; init; }
}

public sealed class AdminContactMessageSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string Subject { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminContactMessageDetailResponse
{
    public required Guid Id { get; init; }

    public Guid? UserId { get; init; }

    public required string Name { get; init; }

    public string? Phone { get; init; }

    public required string Email { get; init; }

    public required string Subject { get; init; }

    public required string Message { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
