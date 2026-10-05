namespace MohammedRaouf.Contracts.Public;

public sealed class CourseSectionPublicResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int SortOrder { get; init; }

    public required int LessonCount { get; init; }

    public required int TotalDurationSeconds { get; init; }

    public required IReadOnlyList<LessonSummaryPublicResponse> Lessons { get; init; }
}

public sealed class LessonSummaryPublicResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required int DurationSeconds { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsFreePreview { get; init; }

    public required string Status { get; init; }
}
