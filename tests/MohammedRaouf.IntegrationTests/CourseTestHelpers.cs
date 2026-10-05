using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

internal static class CourseTestHelpers
{
    public static SaveCourseRequest NewCourseRequest(
        string? slug = null,
        string title = "دورة الاختبار")
    {
        return new SaveCourseRequest
        {
            Title = title,
            Slug = slug ?? $"course-{Guid.NewGuid():N}"[..20],
            ShortDescription = "وصف مختصر للدورة.",
            Description = "وصف تفصيلي كافٍ للدورة.",
            PriceIQD = 250000,
            Level = "Beginner",
            AccessType = "Lifetime",
            IsFeatured = false
        };
    }

    public static async Task<(HttpClient Client, Guid UserId)> LoginAsRoleAsync(
        AuthApiFactory factory,
        string role)
    {
        var client = factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        var user = await AuthTestHelpers.RegisterAsync(client, request);

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var entity = await userManager.FindByIdAsync(user.Id.ToString());
            Assert.NotNull(entity);
            if (!await userManager.IsInRoleAsync(entity, role))
            {
                await userManager.AddToRoleAsync(entity, role);
            }
        }

        var login = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);
        login.EnsureSuccessStatusCode();
        return (client, user.Id);
    }

    public static async Task<Course> SeedPublishedCourseWithLessonsAsync(
        IServiceProvider services,
        bool freePreview = true,
        CourseStatus courseStatus = CourseStatus.Published,
        string? videoKey = "secret-video-key",
        CourseAccessType accessType = CourseAccessType.Lifetime,
        int? accessDurationDays = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "دورة وصول",
            Slug = $"access-{Guid.NewGuid():N}"[..24],
            ShortDescription = "وصف مختصر",
            Description = "وصف تفصيلي",
            PriceIQD = 150000,
            Level = CourseLevel.Beginner,
            Status = courseStatus,
            AccessType = accessType,
            AccessDurationDays = accessDurationDays,
            CreatedAt = now,
            UpdatedAt = now
        };
        var section = new CourseSection
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "القسم الأول",
            SortOrder = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        var preview = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseSectionId = section.Id,
            Title = "درس معاينة",
            Description = "وصف المعاينة",
            DurationSeconds = 120,
            SortOrder = 1,
            IsFreePreview = freePreview,
            Status = LessonStatus.Published,
            VideoKey = videoKey,
            CreatedAt = now,
            UpdatedAt = now
        };
        var paid = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseSectionId = section.Id,
            Title = "درس مدفوع",
            Description = "محتوى مدفوع",
            DurationSeconds = 300,
            SortOrder = 2,
            IsFreePreview = false,
            Status = LessonStatus.Published,
            VideoKey = videoKey,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Courses.Add(course);
        db.CourseSections.Add(section);
        db.Lessons.AddRange(preview, paid);
        await db.SaveChangesAsync();
        course.Sections = [section];
        section.Lessons = [preview, paid];
        return course;
    }

    public static async Task<CourseEnrollment> SeedEnrollmentAsync(
        IServiceProvider services,
        Guid userId,
        Guid courseId,
        EnrollmentStatus status = EnrollmentStatus.Active,
        DateTimeOffset? expiresAt = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTimeOffset.UtcNow;
        var enrollment = new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            Status = status,
            StartedAt = now,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.CourseEnrollments.Add(enrollment);
        await db.SaveChangesAsync();
        return enrollment;
    }

    public static async Task<Lesson> AddLessonAsync(
        IServiceProvider services,
        Guid sectionId,
        string title,
        int sortOrder,
        LessonStatus status = LessonStatus.Published,
        int durationSeconds = 180,
        bool isFreePreview = false)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTimeOffset.UtcNow;
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseSectionId = sectionId,
            Title = title,
            Description = $"وصف {title}",
            DurationSeconds = durationSeconds,
            SortOrder = sortOrder,
            IsFreePreview = isFreePreview,
            Status = status,
            VideoKey = "secret-video-key",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync();
        return lesson;
    }

    public static async Task ArchiveCourseAsync(IServiceProvider services, Guid courseId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var course = await db.Courses.FirstAsync(item => item.Id == courseId);
        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }
}
