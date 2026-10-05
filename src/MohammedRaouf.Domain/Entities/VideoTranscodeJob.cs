using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Domain.Entities;

public class VideoTranscodeJob
{
    public Guid Id { get; set; }

    public Guid LessonId { get; set; }

    public VideoTranscodeStatus Status { get; set; } = VideoTranscodeStatus.Queued;

    public string SourceRelativePath { get; set; } = string.Empty;

    public string? OutputRelativePath { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Lesson Lesson { get; set; } = null!;
}
