using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public VideoProvider VideoProvider { get; set; } = VideoProvider.None;

    public string? VideoKey { get; set; }

    public int DurationSeconds { get; set; }

    public int SortOrder { get; set; }

    public bool IsFreePreview { get; set; }

    public LessonStatus Status { get; set; } = LessonStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public CourseSection CourseSection { get; set; } = null!;

    public ICollection<LessonProgress> ProgressRecords { get; set; } = [];
}
