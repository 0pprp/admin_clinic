using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class CompositeVideoPlaybackService(
    ApplicationDbContext dbContext,
    BunnyStreamPlaybackService bunny,
    SelfHostedHlsPlaybackService selfHosted,
    UnavailableVideoPlaybackService unavailable,
    IOptions<VideoOptions> options) : IVideoPlaybackService
{
    public async Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(item => item.Id == lessonId)
            .Select(item => new { item.Id, item.VideoProvider, item.VideoKey })
            .FirstOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            return null;
        }

        if (lesson.VideoProvider == VideoProvider.SelfHostedHls ||
            (lesson.VideoProvider == VideoProvider.None &&
             !string.IsNullOrWhiteSpace(lesson.VideoKey) &&
             lesson.VideoKey.Contains("master.m3u8", StringComparison.OrdinalIgnoreCase)))
        {
            return await selfHosted.GetPlaybackAsync(lessonId, cancellationToken);
        }

        if (lesson.VideoProvider == VideoProvider.BunnyStream || options.Value.IsBunnyStream)
        {
            return await bunny.GetPlaybackAsync(lessonId, cancellationToken);
        }

        return await unavailable.GetPlaybackAsync(lessonId, cancellationToken);
    }
}

public sealed class SelfHostedHlsPlaybackService(
    ApplicationDbContext dbContext,
    IOptions<VideoOptions> options,
    TimeProvider timeProvider,
    ILogger<SelfHostedHlsPlaybackService> logger) : IVideoPlaybackService
{
    public async Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(item => item.Id == lessonId)
            .Select(item => new { item.Id, item.VideoKey, item.VideoProvider })
            .FirstOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            return null;
        }

        var job = await dbContext.VideoTranscodeJobs.AsNoTracking()
            .Where(item => item.LessonId == lessonId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new { item.Status, item.ErrorMessage, item.OutputRelativePath })
            .FirstOrDefaultAsync(cancellationToken);

        if (job is not null && job.Status is VideoTranscodeStatus.Queued or VideoTranscodeStatus.Processing)
        {
            return new VideoPlaybackResult
            {
                PlaybackUnavailable = true,
                Message = "الفيديو قيد المعالجة الآن. حاول بعد قليل.",
                Provider = "SelfHostedHls",
                Kind = "hls"
            };
        }

        if (job is not null && job.Status == VideoTranscodeStatus.Failed)
        {
            return new VideoPlaybackResult
            {
                PlaybackUnavailable = true,
                Message = string.IsNullOrWhiteSpace(job.ErrorMessage)
                    ? "فشلت معالجة الفيديو. أعد الرفع من لوحة الإدارة."
                    : job.ErrorMessage,
                Provider = "SelfHostedHls",
                Kind = "hls"
            };
        }

        var relative = !string.IsNullOrWhiteSpace(lesson.VideoKey)
            ? lesson.VideoKey.Trim().Replace('\\', '/')
            : job?.OutputRelativePath?.Trim().Replace('\\', '/');

        var settings = options.Value.SelfHosted;
        if (string.IsNullOrWhiteSpace(relative) ||
            string.IsNullOrWhiteSpace(settings.SigningKey) ||
            !relative.EndsWith("master.m3u8", StringComparison.OrdinalIgnoreCase))
        {
            return new VideoPlaybackResult
            {
                PlaybackUnavailable = true,
                Message = "الفيديو غير جاهز للتشغيل بعد.",
                Provider = "SelfHostedHls",
                Kind = "hls"
            };
        }

        var lifetime = options.Value.TokenLifetimeMinutes < 1 ? 10 : options.Value.TokenLifetimeMinutes;
        var expiresAt = timeProvider.GetUtcNow().AddMinutes(lifetime);
        var token = SelfHostedMediaToken.Create(lessonId, expiresAt, settings.SigningKey);
        var url = $"/api/media/v/{token}/lessons/{lessonId:N}/master.m3u8";

        logger.LogInformation("Issued self-hosted HLS playback for lesson {LessonId} expiring at {ExpiresAt}.", lessonId, expiresAt);

        return new VideoPlaybackResult
        {
            PlaybackUnavailable = false,
            Message = "جاهز للتشغيل.",
            PlaybackUrl = url,
            ExpiresAt = expiresAt,
            Provider = "SelfHostedHls",
            Kind = "hls"
        };
    }
}
