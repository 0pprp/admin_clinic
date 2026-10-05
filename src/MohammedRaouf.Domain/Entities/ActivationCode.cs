using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class ActivationCode
{
    public Guid Id { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public Guid PurchaseRequestId { get; set; }

    public ActivationCodeStatus Status { get; set; } = ActivationCodeStatus.Active;

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public PurchaseRequest PurchaseRequest { get; set; } = null!;

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public CourseEnrollment? Enrollment { get; set; }
}
