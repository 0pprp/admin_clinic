using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class CourseAccessService(ApplicationDbContext dbContext, TimeProvider timeProvider) : ICourseAccessService
{
    public async Task<bool> CanUserAccessCourseAsync(
        Guid userId,
        Guid courseId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (IsCourseManager(roles))
        {
            return await dbContext.Courses.AsNoTracking()
                .AnyAsync(course => course.Id == courseId, cancellationToken);
        }

        return await HasActiveEnrollmentAsync(userId, courseId, cancellationToken);
    }

    public async Task<bool> CanUserAccessLessonAsync(
        Guid? userId,
        Guid lessonId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var decision = await EvaluateLessonAccessAsync(userId, lessonId, roles, cancellationToken);
        return decision.Status == LessonAccessStatus.Allowed;
    }

    public async Task<LessonAccessDecision> EvaluateLessonAccessAsync(
        Guid? userId,
        Guid lessonId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(item => item.Id == lessonId)
            .Select(item => new
            {
                item.Id,
                item.Status,
                item.IsFreePreview,
                CourseId = item.CourseSection.CourseId,
                CourseStatus = item.CourseSection.Course.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
        {
            return new LessonAccessDecision(LessonAccessStatus.NotFound);
        }

        if (IsCourseManager(roles))
        {
            return new LessonAccessDecision(LessonAccessStatus.Allowed);
        }

        if (lesson.Status != LessonStatus.Published)
        {
            return new LessonAccessDecision(LessonAccessStatus.NotFound);
        }

        if (userId is Guid authenticatedUser &&
            await HasActiveEnrollmentAsync(authenticatedUser, lesson.CourseId, cancellationToken))
        {
            return new LessonAccessDecision(LessonAccessStatus.Allowed);
        }

        if (lesson.CourseStatus != CourseStatus.Published)
        {
            return userId is null
                ? new LessonAccessDecision(LessonAccessStatus.NotFound)
                : new LessonAccessDecision(LessonAccessStatus.Forbidden);
        }

        if (lesson.IsFreePreview)
        {
            return new LessonAccessDecision(LessonAccessStatus.Allowed);
        }

        return userId is null
            ? new LessonAccessDecision(LessonAccessStatus.Unauthorized)
            : new LessonAccessDecision(LessonAccessStatus.Forbidden);
    }

    private Task<bool> HasActiveEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        return dbContext.CourseEnrollments
            .AsNoTracking()
            .AnyAsync(
                enrollment =>
                    enrollment.UserId == userId &&
                    enrollment.CourseId == courseId &&
                    enrollment.Status == EnrollmentStatus.Active &&
                    (enrollment.ExpiresAt == null || enrollment.ExpiresAt > now),
                cancellationToken);
    }

    private static bool IsCourseManager(IReadOnlyCollection<string> roles) =>
        roles.Contains(RoleNames.Admin, StringComparer.Ordinal) ||
        roles.Contains(RoleNames.ContentManager, StringComparer.Ordinal);
}
