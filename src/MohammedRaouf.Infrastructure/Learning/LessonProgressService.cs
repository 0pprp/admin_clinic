using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Learning;
using MohammedRaouf.Contracts.Learning;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;
using Npgsql;

namespace MohammedRaouf.Infrastructure.Learning;

public sealed class LessonProgressService(
    ApplicationDbContext dbContext,
    ICourseAccessService access,
    TimeProvider timeProvider) : ILessonProgressService
{
    public async Task<ActionResult<LessonProgressResponse>> GetAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await LoadAccessibleLessonAsync(userId, roles, lessonId, cancellationToken);
        if (!lesson.Succeeded)
        {
            return ActionResult<LessonProgressResponse>.Fail(lesson.StatusCode, lesson.Title!, lesson.Detail!);
        }

        var progress = await dbContext.LessonProgress.AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == userId && item.LessonId == lessonId, cancellationToken);
        return ActionResult<LessonProgressResponse>.Ok(Map(lessonId, progress));
    }

    public async Task<ActionResult<LessonProgressResponse>> UpdateWatchedSecondsAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        int watchedSeconds,
        CancellationToken cancellationToken = default)
    {
        var lesson = await LoadAccessibleLessonAsync(userId, roles, lessonId, cancellationToken);
        if (!lesson.Succeeded || lesson.Value is null)
        {
            return ActionResult<LessonProgressResponse>.Fail(lesson.StatusCode, lesson.Title!, lesson.Detail!);
        }

        var clamped = CourseProgressCalculator.ClampWatchedSeconds(watchedSeconds, lesson.Value.DurationSeconds);
        var now = timeProvider.GetUtcNow();
        return await UpsertAsync(
            userId,
            lessonId,
            now,
            existing =>
            {
                existing.WatchedSeconds = Math.Max(existing.WatchedSeconds, clamped);
                existing.LastWatchedAt = now;
                existing.UpdatedAt = now;
            },
            () => new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                WatchedSeconds = clamped,
                IsCompleted = false,
                LastWatchedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            },
            cancellationToken);
    }

    public async Task<ActionResult<LessonProgressResponse>> CompleteAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await LoadAccessibleLessonAsync(userId, roles, lessonId, cancellationToken);
        if (!lesson.Succeeded || lesson.Value is null)
        {
            return ActionResult<LessonProgressResponse>.Fail(lesson.StatusCode, lesson.Title!, lesson.Detail!);
        }

        var now = timeProvider.GetUtcNow();
        return await UpsertAsync(
            userId,
            lessonId,
            now,
            existing =>
            {
                existing.IsCompleted = true;
                existing.CompletedAt ??= now;
                existing.LastWatchedAt = now;
                existing.UpdatedAt = now;
            },
            () => new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                WatchedSeconds = 0,
                IsCompleted = true,
                LastWatchedAt = now,
                CompletedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            },
            cancellationToken);
    }

    private async Task<ActionResult<LessonInfo>> LoadAccessibleLessonAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(item => item.Id == lessonId && item.Status == LessonStatus.Published)
            .Select(item => new LessonInfo(item.Id, item.CourseSection.CourseId, item.DurationSeconds))
            .FirstOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            return ActionResult<LessonInfo>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        if (!await access.CanUserAccessCourseAsync(userId, lesson.CourseId, roles, cancellationToken))
        {
            return ActionResult<LessonInfo>.Fail(403, "ممنوع", "ليس لديك صلاحية للوصول إلى هذا المحتوى.");
        }

        return ActionResult<LessonInfo>.Ok(lesson);
    }

    private async Task<ActionResult<LessonProgressResponse>> UpsertAsync(
        Guid userId,
        Guid lessonId,
        DateTimeOffset now,
        Action<LessonProgress> update,
        Func<LessonProgress> create,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.LessonProgress
            .FirstOrDefaultAsync(item => item.UserId == userId && item.LessonId == lessonId, cancellationToken);
        if (existing is null)
        {
            existing = create();
            dbContext.LessonProgress.Add(existing);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                dbContext.Entry(existing).State = EntityState.Detached;
                existing = await dbContext.LessonProgress
                    .FirstAsync(item => item.UserId == userId && item.LessonId == lessonId, cancellationToken);
                update(existing);
                existing.UpdatedAt = now;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            update(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ActionResult<LessonProgressResponse>.Ok(Map(lessonId, existing));
    }

    private static LessonProgressResponse Map(Guid lessonId, LessonProgress? progress) =>
        new()
        {
            LessonId = lessonId,
            WatchedSeconds = progress?.WatchedSeconds ?? 0,
            IsCompleted = progress?.IsCompleted ?? false,
            LastWatchedAt = progress?.LastWatchedAt,
            CompletedAt = progress?.CompletedAt
        };

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private sealed record LessonInfo(Guid Id, Guid CourseId, int DurationSeconds);
}
