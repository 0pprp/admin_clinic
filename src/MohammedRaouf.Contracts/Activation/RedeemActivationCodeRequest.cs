namespace MohammedRaouf.Contracts.Activation;

public sealed class RedeemActivationCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public sealed class RedeemActivationCodeResponse
{
    public required Guid EnrollmentId { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required string Status { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed class IssuedActivationCodeResponse
{
    public required string ActivationCode { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string RequestNumber { get; init; }
}

public sealed class DirectActivationResponse
{
    public required Guid EnrollmentId { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required string Status { get; init; }

    public required string RequestNumber { get; init; }
}

public sealed class AdminActivationCodeSummaryResponse
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string UserEmail { get; init; }

    public required string UserFullName { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required Guid PurchaseRequestId { get; init; }

    public required string RequestNumber { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public DateTimeOffset? UsedAt { get; init; }

    public required Guid CreatedBy { get; init; }
}

public sealed class StudentEnrollmentResponse
{
    public required Guid Id { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseSlug { get; init; }

    public required string CourseTitle { get; init; }

    public string? ThumbnailUrl { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public required string AccessType { get; init; }

    public required bool CanAccess { get; init; }

    public required int ProgressPercent { get; init; }

    public required int CompletedLessons { get; init; }

    public required int TotalLessons { get; init; }

    public Guid? ContinueLessonId { get; init; }

    public string? ContinueLessonTitle { get; init; }
}
