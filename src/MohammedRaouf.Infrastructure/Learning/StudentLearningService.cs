using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Learning;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Learning;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Learning;

public sealed class StudentLearningService(
    ApplicationDbContext dbContext,
    ICourseAccessService access,
    TimeProvider timeProvider) : IStudentLearningService, IEnrollmentQueryService
{
    private static readonly PurchaseRequestStatus[] OpenPurchaseStatuses =
    [
        PurchaseRequestStatus.Pending,
        PurchaseRequestStatus.Contacted,
        PurchaseRequestStatus.AwaitingPayment,
        PurchaseRequestStatus.PaymentReceived,
        PurchaseRequestStatus.ActivationCodeIssued
    ];

    public Task<IReadOnlyList<StudentEnrollmentResponse>> ListMineAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ListMyCoursesAsync(userId, cancellationToken);

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var enrollments = await LoadEnrollmentsAsync(userId, cancellationToken);
        var accessible = enrollments.Where(item => item.CanAccess(now)).ToList();
        var snapshots = await LoadSnapshotsAsync(userId, enrollments.Select(item => item.CourseId).ToArray(), cancellationToken);

        var continueLearning = PickDashboardContinue(accessible, snapshots);
        var activityTimes = snapshots.Values
            .Select(item => item.LastActivity)
            .Where(item => item is not null)
            .ToList();
        var lastActivity = activityTimes.Count == 0 ? null : activityTimes.Max();

        var openPurchases = await dbContext.PurchaseRequests.AsNoTracking()
            .CountAsync(
                item => item.UserId == userId && OpenPurchaseStatuses.Contains(item.Status),
                cancellationToken);

        var accessibleIds = accessible.Select(item => item.CourseId).ToHashSet();
        var completed = snapshots
            .Where(pair => accessibleIds.Contains(pair.Key))
            .Sum(pair => pair.Value.Completed);
        var total = snapshots
            .Where(pair => accessibleIds.Contains(pair.Key))
            .Sum(pair => pair.Value.Total);

        return new DashboardSummaryResponse
        {
            ActiveCoursesCount = accessible.Count,
            CompletedLessonsCount = completed,
            TotalAccessibleLessonsCount = total,
            OpenPurchaseRequestsCount = openPurchases,
            LastLearningActivity = lastActivity,
            ContinueLearning = continueLearning
        };
    }

    public async Task<IReadOnlyList<StudentEnrollmentResponse>> ListMyCoursesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var enrollments = await LoadEnrollmentsAsync(userId, cancellationToken);
        var snapshots = await LoadSnapshotsAsync(userId, enrollments.Select(item => item.CourseId).ToArray(), cancellationToken);

        return enrollments.Select(item =>
        {
            snapshots.TryGetValue(item.CourseId, out var snapshot);
            snapshot ??= CourseSnapshot.Empty;
            var continueLesson = item.CanAccess(now) ? snapshot.ContinueLesson : null;
            return new StudentEnrollmentResponse
            {
                Id = item.Id,
                CourseId = item.CourseId,
                CourseSlug = item.Slug,
                CourseTitle = item.Title,
                ThumbnailUrl = item.ThumbnailUrl,
                Status = item.Status.ToString(),
                StartedAt = item.StartedAt,
                ExpiresAt = item.ExpiresAt,
                AccessType = item.AccessType.ToString(),
                CanAccess = item.CanAccess(now),
                ProgressPercent = snapshot.Percent,
                CompletedLessons = snapshot.Completed,
                TotalLessons = snapshot.Total,
                ContinueLessonId = continueLesson?.LessonId,
                ContinueLessonTitle = continueLesson?.LessonTitle
            };
        }).ToList();
    }

    public async Task<ActionResult<StudentCourseLearningResponse>> GetCourseAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        string courseSlug,
        CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.AsNoTracking()
            .Where(item => item.Slug == courseSlug)
            .Select(item => new { item.Id, item.Slug, item.Title, item.Description, item.ThumbnailUrl, item.AccessType })
            .FirstOrDefaultAsync(cancellationToken);
        if (course is null)
        {
            return ActionResult<StudentCourseLearningResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        if (!await access.CanUserAccessCourseAsync(userId, course.Id, roles, cancellationToken))
        {
            return ActionResult<StudentCourseLearningResponse>.Fail(
                403,
                "ممنوع",
                "ليس لديك صلاحية للوصول إلى هذه الدورة.");
        }

        var enrollment = await dbContext.CourseEnrollments.AsNoTracking()
            .Where(item => item.UserId == userId && item.CourseId == course.Id)
            .Select(item => new { item.StartedAt, item.ExpiresAt })
            .FirstOrDefaultAsync(cancellationToken);

        var snapshot = (await LoadSnapshotsAsync(userId, [course.Id], cancellationToken))
            .GetValueOrDefault(course.Id, CourseSnapshot.Empty);

        return ActionResult<StudentCourseLearningResponse>.Ok(new StudentCourseLearningResponse
        {
            Id = course.Id,
            Slug = course.Slug,
            Title = course.Title,
            Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl,
            AccessType = course.AccessType.ToString(),
            StartedAt = enrollment?.StartedAt ?? timeProvider.GetUtcNow(),
            ExpiresAt = enrollment?.ExpiresAt,
            ProgressPercent = snapshot.Percent,
            CompletedLessons = snapshot.Completed,
            TotalLessons = snapshot.Total,
            CourseCompleted = snapshot.Total > 0 && snapshot.Completed == snapshot.Total,
            ContinueLesson = snapshot.ContinueLesson is null
                ? null
                : new ContinueLearningResponse
                {
                    CourseId = course.Id,
                    CourseSlug = course.Slug,
                    CourseTitle = course.Title,
                    LessonId = snapshot.ContinueLesson.LessonId,
                    LessonTitle = snapshot.ContinueLesson.LessonTitle,
                    LastWatchedAt = snapshot.ContinueLesson.LastWatchedAt
                },
            Sections = snapshot.Sections
        });
    }

    public async Task<ActionResult<StudentLessonLearningResponse>> GetLessonAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        string courseSlug,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.AsNoTracking()
            .Where(item => item.Id == lessonId && item.Status == LessonStatus.Published)
            .Select(item => new
            {
                item.Id,
                item.Title,
                item.Description,
                item.DurationSeconds,
                CourseId = item.CourseSection.CourseId,
                CourseSlug = item.CourseSection.Course.Slug,
                CourseTitle = item.CourseSection.Course.Title
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            return ActionResult<StudentLessonLearningResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        if (!string.Equals(lesson.CourseSlug, courseSlug, StringComparison.OrdinalIgnoreCase))
        {
            return ActionResult<StudentLessonLearningResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        if (!await access.CanUserAccessCourseAsync(userId, lesson.CourseId, roles, cancellationToken))
        {
            return ActionResult<StudentLessonLearningResponse>.Fail(
                403,
                "ممنوع",
                "ليس لديك صلاحية للوصول إلى هذا المحتوى.");
        }

        var snapshot = (await LoadSnapshotsAsync(userId, [lesson.CourseId], cancellationToken))
            .GetValueOrDefault(lesson.CourseId, CourseSnapshot.Empty);
        var flat = snapshot.Sections.SelectMany(section => section.Lessons).ToList();
        var index = flat.FindIndex(item => item.Id == lesson.Id);
        var current = index >= 0 ? flat[index] : null;
        var previous = index > 0 ? flat[index - 1] : null;
        var next = index >= 0 && index < flat.Count - 1 ? flat[index + 1] : null;

        return ActionResult<StudentLessonLearningResponse>.Ok(new StudentLessonLearningResponse
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds,
            CourseId = lesson.CourseId,
            CourseSlug = lesson.CourseSlug,
            CourseTitle = lesson.CourseTitle,
            ProgressPercent = snapshot.Percent,
            IsCompleted = current?.IsCompleted ?? false,
            WatchedSeconds = current?.WatchedSeconds ?? 0,
            LastWatchedAt = current?.LastWatchedAt,
            PreviousLesson = previous is null ? null : new LessonNeighborResponse { Id = previous.Id, Title = previous.Title },
            NextLesson = next is null ? null : new LessonNeighborResponse { Id = next.Id, Title = next.Title },
            Sections = snapshot.Sections
        });
    }

    private async Task<List<EnrollmentRow>> LoadEnrollmentsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.CourseEnrollments.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.StartedAt)
            .Select(item => new EnrollmentRow(
                item.Id,
                item.CourseId,
                item.Course.Slug,
                item.Course.Title,
                item.Course.ThumbnailUrl,
                item.Status,
                item.StartedAt,
                item.ExpiresAt,
                item.Course.AccessType))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, CourseSnapshot>> LoadSnapshotsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> courseIds,
        CancellationToken cancellationToken)
    {
        if (courseIds.Count == 0)
        {
            return [];
        }

        var lessons = await dbContext.Lessons.AsNoTracking()
            .Where(item =>
                item.Status == LessonStatus.Published &&
                courseIds.Contains(item.CourseSection.CourseId))
            .Select(item => new LessonRow(
                item.Id,
                item.Title,
                item.Description,
                item.DurationSeconds,
                item.SortOrder,
                item.CourseSection.CourseId,
                item.CourseSectionId,
                item.CourseSection.Title,
                item.CourseSection.SortOrder))
            .ToListAsync(cancellationToken);

        var lessonIds = lessons.Select(item => item.Id).ToArray();
        var progress = lessonIds.Length == 0
            ? []
            : await dbContext.LessonProgress.AsNoTracking()
                .Where(item => item.UserId == userId && lessonIds.Contains(item.LessonId))
                .Select(item => new ProgressRow(item.LessonId, item.WatchedSeconds, item.IsCompleted, item.LastWatchedAt))
                .ToListAsync(cancellationToken);

        var progressMap = progress.ToDictionary(item => item.LessonId);
        return lessons
            .GroupBy(item => item.CourseId)
            .ToDictionary(group => group.Key, group => BuildSnapshot(group.Key, group.ToList(), progressMap));
    }

    private static CourseSnapshot BuildSnapshot(
        Guid courseId,
        List<LessonRow> lessons,
        IReadOnlyDictionary<Guid, ProgressRow> progressMap)
    {
        var ordered = lessons
            .OrderBy(item => item.SectionSortOrder)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .ToList();

        var sections = ordered
            .GroupBy(item => new { item.SectionId, item.SectionTitle, item.SectionSortOrder })
            .OrderBy(group => group.Key.SectionSortOrder)
            .ThenBy(group => group.Key.SectionTitle)
            .Select(group => new StudentLearningSectionResponse
            {
                Id = group.Key.SectionId,
                Title = group.Key.SectionTitle,
                SortOrder = group.Key.SectionSortOrder,
                Lessons = group.Select(item =>
                {
                    progressMap.TryGetValue(item.Id, out var progress);
                    return new StudentLearningLessonSummaryResponse
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Description = ShortDescription(item.Description),
                        DurationSeconds = item.DurationSeconds,
                        SortOrder = item.SortOrder,
                        IsCompleted = progress?.IsCompleted ?? false,
                        WatchedSeconds = progress?.WatchedSeconds ?? 0,
                        LastWatchedAt = progress?.LastWatchedAt,
                        CanAccess = true
                    };
                }).ToList()
            })
            .ToList();

        var flat = sections.SelectMany(section => section.Lessons).ToList();
        var completed = flat.Count(item => item.IsCompleted);
        var continueLesson = PickContinue(flat);

        return new CourseSnapshot(
            completed,
            flat.Count,
            CourseProgressCalculator.Percent(completed, flat.Count),
            continueLesson,
            sections,
            flat.Count == 0 ? null : flat.Max(item => item.LastWatchedAt));
    }

    private static string? ShortDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length <= 180)
        {
            return description;
        }

        return description[..177] + "...";
    }

    private static ContinueCandidate? PickContinue(IReadOnlyList<StudentLearningLessonSummaryResponse> lessons)
    {
        if (lessons.Count == 0)
        {
            return null;
        }

        var lastIncomplete = lessons
            .Where(item => !item.IsCompleted && item.LastWatchedAt is not null)
            .OrderByDescending(item => item.LastWatchedAt)
            .FirstOrDefault();
        if (lastIncomplete is not null)
        {
            return new ContinueCandidate(lastIncomplete.Id, lastIncomplete.Title, lastIncomplete.LastWatchedAt);
        }

        var firstIncomplete = lessons.FirstOrDefault(item => !item.IsCompleted);
        var target = firstIncomplete ?? lessons[0];
        return new ContinueCandidate(target.Id, target.Title, target.LastWatchedAt);
    }

    private static ContinueLearningResponse? PickDashboardContinue(
        IReadOnlyList<EnrollmentRow> accessible,
        IReadOnlyDictionary<Guid, CourseSnapshot> snapshots)
    {
        var candidates = accessible
            .Select(item =>
            {
                snapshots.TryGetValue(item.CourseId, out var snapshot);
                if (snapshot?.ContinueLesson is null)
                {
                    return null;
                }

                return new ContinueLearningResponse
                {
                    CourseId = item.CourseId,
                    CourseSlug = item.Slug,
                    CourseTitle = item.Title,
                    LessonId = snapshot.ContinueLesson.LessonId,
                    LessonTitle = snapshot.ContinueLesson.LessonTitle,
                    LastWatchedAt = snapshot.ContinueLesson.LastWatchedAt
                };
            })
            .OfType<ContinueLearningResponse>()
            .ToList();

        return candidates
            .OrderByDescending(item => item.LastWatchedAt)
            .ThenByDescending(item => accessible.First(row => row.CourseId == item.CourseId).StartedAt)
            .FirstOrDefault();
    }

    private sealed record EnrollmentRow(
        Guid Id,
        Guid CourseId,
        string Slug,
        string Title,
        string? ThumbnailUrl,
        EnrollmentStatus Status,
        DateTimeOffset StartedAt,
        DateTimeOffset? ExpiresAt,
        CourseAccessType AccessType)
    {
        public bool CanAccess(DateTimeOffset now) =>
            Status == EnrollmentStatus.Active && (ExpiresAt is null || ExpiresAt > now);
    }

    private sealed record LessonRow(
        Guid Id,
        string Title,
        string? Description,
        int DurationSeconds,
        int SortOrder,
        Guid CourseId,
        Guid SectionId,
        string SectionTitle,
        int SectionSortOrder);

    private sealed record ProgressRow(Guid LessonId, int WatchedSeconds, bool IsCompleted, DateTimeOffset? LastWatchedAt);

    private sealed record ContinueCandidate(Guid LessonId, string LessonTitle, DateTimeOffset? LastWatchedAt);

    private sealed record CourseSnapshot(
        int Completed,
        int Total,
        int Percent,
        ContinueCandidate? ContinueLesson,
        IReadOnlyList<StudentLearningSectionResponse> Sections,
        DateTimeOffset? LastActivity)
    {
        public static CourseSnapshot Empty { get; } = new(0, 0, 0, null, [], null);
    }
}
