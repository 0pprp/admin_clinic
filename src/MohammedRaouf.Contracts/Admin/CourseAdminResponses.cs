namespace MohammedRaouf.Contracts.Admin;

public sealed class AdminCourseSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Status { get; init; }

    public required string Level { get; init; }

    public required long PriceIQD { get; init; }

    public required bool IsFeatured { get; init; }

    public required int SectionCount { get; init; }

    public required int LessonCount { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class AdminCourseDetailResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string ShortDescription { get; init; }

    public required string Description { get; init; }

    public required long PriceIQD { get; init; }

    public string? ThumbnailUrl { get; init; }

    public string? TrailerUrl { get; init; }

    public required string Level { get; init; }

    public required string Status { get; init; }

    public required bool IsFeatured { get; init; }

    public required string AccessType { get; init; }

    public int? AccessDurationDays { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required IReadOnlyList<AdminSectionResponse> Sections { get; init; }
}

public sealed class AdminSectionResponse
{
    public required Guid Id { get; init; }

    public required Guid CourseId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int SortOrder { get; init; }

    public required IReadOnlyList<AdminLessonResponse> Lessons { get; init; }
}

public sealed class AdminLessonResponse
{
    public required Guid Id { get; init; }

    public required Guid SectionId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int DurationSeconds { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsFreePreview { get; init; }

    public required string Status { get; init; }

    public required string VideoProvider { get; init; }

    public string? VideoKey { get; init; }

    public string? VideoProcessingStatus { get; init; }

    public string? VideoProcessingMessage { get; init; }
}
