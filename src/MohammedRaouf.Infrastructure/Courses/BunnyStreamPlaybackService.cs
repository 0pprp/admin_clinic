using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class BunnyStreamPlaybackService(
    ApplicationDbContext dbContext,
    IOptions<VideoOptions> options,
    TimeProvider timeProvider,
    ILogger<BunnyStreamPlaybackService> logger) : IVideoPlaybackService
{
    public async Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(item => item.Id == lessonId)
            .Select(item => new { item.Id, item.VideoKey })
            .FirstOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            return null;
        }

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(lesson.VideoKey) ||
            string.IsNullOrWhiteSpace(settings.Bunny.LibraryId) ||
            string.IsNullOrWhiteSpace(settings.Bunny.TokenKey))
        {
            return new VideoPlaybackResult
            {
                PlaybackUnavailable = true,
                Message = "الفيديو غير جاهز للتشغيل بعد.",
                Provider = "BunnyStream"
            };
        }

        var lifetime = settings.TokenLifetimeMinutes < 1 ? 10 : settings.TokenLifetimeMinutes;
        var expiresAt = timeProvider.GetUtcNow().AddMinutes(lifetime);
        var url = BunnyStreamUrlSigner.CreateEmbedUrl(
            settings.Bunny.LibraryId,
            lesson.VideoKey.Trim(),
            settings.Bunny.TokenKey,
            expiresAt,
            settings.Bunny.CdnHostname);

        logger.LogInformation("Issued Bunny Stream playback for lesson {LessonId} expiring at {ExpiresAt}.", lessonId, expiresAt);

        return new VideoPlaybackResult
        {
            PlaybackUnavailable = false,
            Message = "جاهز للتشغيل.",
            PlaybackUrl = url,
            ExpiresAt = expiresAt,
            Provider = "BunnyStream",
            Kind = "iframe"
        };
    }
}
