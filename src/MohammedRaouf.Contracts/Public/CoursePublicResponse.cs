namespace MohammedRaouf.Contracts.Public;

public sealed class CourseSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string ShortDescription { get; init; }

    public required long PriceIQD { get; init; }

    public string? ThumbnailUrl { get; init; }

    public required string Level { get; init; }
}

public sealed class CourseDetailResponse
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

    public required string AccessType { get; init; }

    public int? AccessDurationDays { get; init; }

    public required int SectionCount { get; init; }

    public required int LessonCount { get; init; }

    public required int TotalDurationSeconds { get; init; }

    public required IReadOnlyList<CourseSectionPublicResponse> Sections { get; init; }
}
