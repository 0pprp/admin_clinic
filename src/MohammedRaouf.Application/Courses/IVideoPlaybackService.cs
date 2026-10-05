namespace MohammedRaouf.Application.Courses;

public sealed class VideoPlaybackResult
{
    public bool PlaybackUnavailable { get; init; } = true;

    public string Message { get; init; } = "المعاينة المرئية ستتوفر عند ربط مزود الفيديو.";

    public string? PlaybackUrl { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public string? Provider { get; init; }

    public string? Kind { get; init; }
}

public interface IVideoPlaybackService
{
    Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
