namespace MohammedRaouf.Contracts.Learning;

public sealed class DashboardSummaryResponse
{
    public required int ActiveCoursesCount { get; init; }

    public required int CompletedLessonsCount { get; init; }

    public required int TotalAccessibleLessonsCount { get; init; }

    public required int OpenPurchaseRequestsCount { get; init; }

    public DateTimeOffset? LastLearningActivity { get; init; }

    public ContinueLearningResponse? ContinueLearning { get; init; }
}

public sealed class ContinueLearningResponse
{
    public required Guid CourseId { get; init; }

    public required string CourseSlug { get; init; }

    public required string CourseTitle { get; init; }

    public required Guid LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public DateTimeOffset? LastWatchedAt { get; init; }
}

public sealed class StudentCourseLearningResponse
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public string? ThumbnailUrl { get; init; }

    public required string AccessType { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public required int ProgressPercent { get; init; }

    public required int CompletedLessons { get; init; }

    public required int TotalLessons { get; init; }

    public required bool CourseCompleted { get; init; }

    public ContinueLearningResponse? ContinueLesson { get; init; }

    public required IReadOnlyList<StudentLearningSectionResponse> Sections { get; init; }
}

public sealed class StudentLessonLearningResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int DurationSeconds { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseSlug { get; init; }

    public required string CourseTitle { get; init; }

    public required int ProgressPercent { get; init; }

    public required bool IsCompleted { get; init; }

    public required int WatchedSeconds { get; init; }

    public DateTimeOffset? LastWatchedAt { get; init; }

    public LessonNeighborResponse? PreviousLesson { get; init; }

    public LessonNeighborResponse? NextLesson { get; init; }

    public required IReadOnlyList<StudentLearningSectionResponse> Sections { get; init; }
}

public sealed class StudentLearningSectionResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required int SortOrder { get; init; }

    public required IReadOnlyList<StudentLearningLessonSummaryResponse> Lessons { get; init; }
}

public sealed class StudentLearningLessonSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int DurationSeconds { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsCompleted { get; init; }

    public required int WatchedSeconds { get; init; }

    public DateTimeOffset? LastWatchedAt { get; init; }

    public required bool CanAccess { get; init; }
}

public sealed class LessonNeighborResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }
}

public sealed class LessonProgressResponse
{
    public required Guid LessonId { get; init; }

    public required int WatchedSeconds { get; init; }

    public required bool IsCompleted { get; init; }

    public DateTimeOffset? LastWatchedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }
}

public sealed class UpdateLessonProgressRequest
{
    public int WatchedSeconds { get; set; }
}
