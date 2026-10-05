using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class CourseEnrollment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }

    public Guid? PurchaseRequestId { get; set; }

    public Guid? ActivationCodeId { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid? ActivatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public PurchaseRequest? PurchaseRequest { get; set; }

    public ActivationCode? ActivationCode { get; set; }

    public ApplicationUser? ActivatedByUser { get; set; }
}
