using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class LessonProgress
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }

    public int WatchedSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? LastWatchedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Lesson Lesson { get; set; } = null!;
}
