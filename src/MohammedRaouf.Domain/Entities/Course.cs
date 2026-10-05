using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long PriceIQD { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? TrailerUrl { get; set; }

    public CourseLevel Level { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public bool IsFeatured { get; set; }

    public CourseAccessType AccessType { get; set; } = CourseAccessType.Lifetime;

    public int? AccessDurationDays { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<CourseSection> Sections { get; set; } = [];

    public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = [];

    public ICollection<CourseEnrollment> Enrollments { get; set; } = [];

    public ICollection<ActivationCode> ActivationCodes { get; set; } = [];
}
