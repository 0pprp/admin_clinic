using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Courses;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class CourseQueryService(ApplicationDbContext dbContext) : ICourseQueryService
{
    public async Task<LessonPreviewResponse?> GetPublicPreviewAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Lessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.Id == lessonId &&
                lesson.IsFreePreview &&
                lesson.Status == LessonStatus.Published &&
                lesson.CourseSection.Course.Status == CourseStatus.Published)
            .Select(lesson => new LessonPreviewResponse
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                DurationSeconds = lesson.DurationSeconds,
                PreviewAvailable = true,
                CourseId = lesson.CourseSection.CourseId,
                CourseSlug = lesson.CourseSection.Course.Slug,
                CourseTitle = lesson.CourseSection.Course.Title
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LessonMetadataResponse?> GetLessonMetadataAsync(
        Guid lessonId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(item => item.Id == lessonId)
            .Select(item => new LessonMetadataResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                DurationSeconds = item.DurationSeconds,
                SectionId = item.CourseSectionId,
                SectionTitle = item.CourseSection.Title,
                CourseId = item.CourseSection.CourseId,
                CourseTitle = item.CourseSection.Course.Title,
                CourseSlug = item.CourseSection.Course.Slug,
                IsFreePreview = item.IsFreePreview
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
        {
            return null;
        }

        bool? completed = null;
        if (userId is Guid authenticatedUser)
        {
            completed = await dbContext.LessonProgress
                .AsNoTracking()
                .Where(progress => progress.UserId == authenticatedUser && progress.LessonId == lessonId)
                .Select(progress => progress.IsCompleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new LessonMetadataResponse
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds,
            SectionId = lesson.SectionId,
            SectionTitle = lesson.SectionTitle,
            CourseId = lesson.CourseId,
            CourseTitle = lesson.CourseTitle,
            CourseSlug = lesson.CourseSlug,
            IsFreePreview = lesson.IsFreePreview,
            IsCompleted = completed
        };
    }

    public async Task<CourseOutlineResponse?> GetStudentOutlineAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Include(item => item.Sections)
            .ThenInclude(section => section.Lessons)
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);

        if (course is null)
        {
            return null;
        }

        return new CourseOutlineResponse
        {
            CourseId = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            Status = course.Status.ToString(),
            Sections = MapPublishedCurriculum(course.Sections)
        };
    }

    public static IReadOnlyList<CourseSectionPublicResponse> MapPublishedCurriculum(
        IEnumerable<Domain.Entities.CourseSection> sections)
    {
        return sections
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Title)
            .Select(section =>
            {
                var lessons = section.Lessons
                    .Where(lesson => lesson.Status == LessonStatus.Published)
                    .OrderBy(lesson => lesson.SortOrder)
                    .ThenBy(lesson => lesson.Title)
                    .Select(lesson => new LessonSummaryPublicResponse
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        DurationSeconds = lesson.DurationSeconds,
                        SortOrder = lesson.SortOrder,
                        IsFreePreview = lesson.IsFreePreview,
                        Status = lesson.Status.ToString()
                    })
                    .ToList();

                return new CourseSectionPublicResponse
                {
                    Id = section.Id,
                    Title = section.Title,
                    Description = section.Description,
                    SortOrder = section.SortOrder,
                    LessonCount = lessons.Count,
                    TotalDurationSeconds = lessons.Sum(lesson => lesson.DurationSeconds),
                    Lessons = lessons
                };
            })
            .Where(section => section.LessonCount > 0)
            .ToList();
    }
}
