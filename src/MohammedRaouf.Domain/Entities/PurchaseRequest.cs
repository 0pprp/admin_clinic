using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class PurchaseRequest
{
    public Guid Id { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Governorate { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentReference { get; set; }

    public long AmountIQD { get; set; }

    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Pending;

    public string? CustomerNotes { get; set; }

    public string? AdminNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ContactedAt { get; set; }

    public DateTimeOffset? PaymentReceivedAt { get; set; }

    public Guid? ConfirmedBy { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public ApplicationUser? ConfirmedByUser { get; set; }

    public ICollection<PurchaseRequestEvent> Events { get; set; } = [];

    public ICollection<ActivationCode> ActivationCodes { get; set; } = [];

    public CourseEnrollment? Enrollment { get; set; }
}
