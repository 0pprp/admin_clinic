namespace MohammedRaouf.Contracts.Consultations;

public sealed class CreateConsultationRequest
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public string? Email { get; set; }

    public string ConsultationType { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public DateOnly? PreferredDate { get; set; }

    public TimeOnly? PreferredTime { get; set; }

    public string Topic { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string PreferredCommunicationMethod { get; set; } = string.Empty;

    public string? Website { get; set; }
}

public sealed class CreateConsultationResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required string Status { get; init; }
}

public sealed class ScheduleConsultationRequest
{
    public DateOnly ScheduledDate { get; set; }

    public TimeOnly ScheduledTime { get; set; }

    public string? AdminNotes { get; set; }
}

public sealed class ConsultationNoteRequest
{
    public string? AdminNotes { get; set; }
}

public sealed class AdminConsultationSummaryResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required string FullName { get; init; }

    public required string PhoneNumber { get; init; }

    public required string ConsultationType { get; init; }

    public DateOnly? PreferredDate { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminConsultationDetailResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public Guid? UserId { get; init; }

    public required string FullName { get; init; }

    public required string PhoneNumber { get; init; }

    public string? WhatsAppNumber { get; init; }

    public string? Email { get; init; }

    public required string ConsultationType { get; init; }

    public string? CompanyName { get; init; }

    public DateOnly? PreferredDate { get; init; }

    public TimeOnly? PreferredTime { get; init; }

    public required string Topic { get; init; }

    public required string Message { get; init; }

    public required string PreferredCommunicationMethod { get; init; }

    public required string Status { get; init; }

    public DateTimeOffset? ScheduledAt { get; init; }

    public string? AdminNotes { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
