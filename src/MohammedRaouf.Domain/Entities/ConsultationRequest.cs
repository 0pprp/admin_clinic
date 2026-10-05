using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class ConsultationRequest
{
    public Guid Id { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public Guid? UserId { get; set; }

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

    public ConsultationStatus Status { get; set; } = ConsultationStatus.New;

    public DateTimeOffset? ScheduledAt { get; set; }

    public string? AdminNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }
}
