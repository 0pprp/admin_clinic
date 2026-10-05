using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class UnavailableVideoPlaybackService(ApplicationDbContext dbContext) : IVideoPlaybackService
{
    public async Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Lessons.AsNoTracking()
            .AnyAsync(lesson => lesson.Id == lessonId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        return new VideoPlaybackResult
        {
            PlaybackUnavailable = true,
            Message = "المعاينة المرئية ستتوفر عند ربط مزود الفيديو."
        };
    }
}
