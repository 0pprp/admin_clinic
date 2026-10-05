namespace MohammedRaouf.Contracts.Courses;

public sealed class LessonPreviewResponse
{
    public required Guid LessonId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int DurationSeconds { get; init; }

    public required bool PreviewAvailable { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseSlug { get; init; }

    public required string CourseTitle { get; init; }
}

public sealed class LessonMetadataResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int DurationSeconds { get; init; }

    public required Guid SectionId { get; init; }

    public required string SectionTitle { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required bool IsFreePreview { get; init; }

    public bool? IsCompleted { get; init; }
}

public sealed class LessonPlaybackResponse
{
    public required Guid LessonId { get; init; }

    public required bool PlaybackUnavailable { get; init; }

    public required string Message { get; init; }

    public string? PlaybackUrl { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public string? Provider { get; init; }

    public string? Kind { get; init; }
}
