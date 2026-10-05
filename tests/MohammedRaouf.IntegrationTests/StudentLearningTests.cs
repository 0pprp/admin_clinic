using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Contracts.Learning;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class StudentLearningTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = AuthTestHelpers.JsonOptions;
    private readonly AuthApiFactory _factory;

    public StudentLearningTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Dashboard_summary_returns_counts_from_accessible_courses()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        var summary = await client.GetFromJsonAsync<DashboardSummaryResponse>("/api/dashboard/summary", JsonOptions);

        Assert.NotNull(summary);
        Assert.Equal(1, summary.ActiveCoursesCount);
        Assert.Equal(0, summary.CompletedLessonsCount);
        Assert.Equal(2, summary.TotalAccessibleLessonsCount);
        Assert.Equal(0, summary.OpenPurchaseRequestsCount);
        Assert.NotNull(summary.ContinueLearning);
        Assert.Equal(course.Id, summary.ContinueLearning.CourseId);
        Assert.Equal(FirstLesson(course).Id, summary.ContinueLearning.LessonId);
    }

    [Fact]
    public async Task My_courses_lists_enrollments_with_progress()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        var items = await client.GetFromJsonAsync<IReadOnlyList<StudentEnrollmentResponse>>("/api/enrollments", JsonOptions);

        Assert.NotNull(items);
        var item = Assert.Single(items);
        Assert.Equal(course.Slug, item.CourseSlug);
        Assert.True(item.CanAccess);
        Assert.Equal(0, item.ProgressPercent);
        Assert.Equal(2, item.TotalLessons);
        Assert.Equal("Active", item.Status);
    }

    [Fact]
    public async Task Two_courses_remain_visible_and_progress_is_independent()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var courseA = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var sectionA = courseA.Sections.Single().Id;
        var sectionB = courseB.Sections.Single().Id;
        await CourseTestHelpers.AddLessonAsync(_factory.Services, sectionA, "درس أ3", 3);
        await CourseTestHelpers.AddLessonAsync(_factory.Services, sectionA, "درس أ4", 4);
        await CourseTestHelpers.AddLessonAsync(_factory.Services, sectionB, "درس ب3", 3);
        await CourseTestHelpers.AddLessonAsync(_factory.Services, sectionB, "درس ب4", 4);
        await CourseTestHelpers.AddLessonAsync(_factory.Services, sectionB, "درس ب5", 5);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, courseA.Id);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, courseB.Id);

        var uniqueA = await PublishedLessonIdsAsync(courseA.Id);
        var uniqueB = await PublishedLessonIdsAsync(courseB.Id);
        Assert.Equal(4, uniqueA.Count);
        Assert.Equal(5, uniqueB.Count);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{uniqueA[0]}/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{uniqueA[1]}/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{uniqueB[0]}/complete", null)).StatusCode);

        var items = await client.GetFromJsonAsync<IReadOnlyList<StudentEnrollmentResponse>>("/api/enrollments", JsonOptions);
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        var cardA = items.Single(item => item.CourseId == courseA.Id);
        var cardB = items.Single(item => item.CourseId == courseB.Id);
        Assert.Equal(50, cardA.ProgressPercent);
        Assert.Equal(2, cardA.CompletedLessons);
        Assert.Equal(4, cardA.TotalLessons);
        Assert.Equal(20, cardB.ProgressPercent);
        Assert.Equal(1, cardB.CompletedLessons);
        Assert.Equal(5, cardB.TotalLessons);
    }

    [Fact]
    public async Task Archived_course_with_active_enrollment_stays_visible_and_learnable()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);
        await CourseTestHelpers.ArchiveCourseAsync(_factory.Services, course.Id);
        var lessonId = FirstLesson(course).Id;

        var items = await client.GetFromJsonAsync<IReadOnlyList<StudentEnrollmentResponse>>("/api/enrollments", JsonOptions);
        Assert.Contains(items!, item => item.CourseId == course.Id && item.CanAccess);

        var learning = await client.GetAsync($"/api/student/courses/{course.Slug}");
        var lesson = await client.GetAsync($"/api/student/courses/{course.Slug}/lessons/{lessonId}");
        var playback = await client.GetAsync($"/api/lessons/{lessonId}/playback");
        Assert.Equal(HttpStatusCode.OK, learning.StatusCode);
        Assert.Equal(HttpStatusCode.OK, lesson.StatusCode);
        Assert.Equal(HttpStatusCode.OK, playback.StatusCode);
    }

    [Fact]
    public async Task Suspended_expired_and_revoked_enrollments_are_denied()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.Single().Lessons.Single(item => !item.IsFreePreview);

        var (suspended, suspendedId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, suspendedId, course.Id, EnrollmentStatus.Suspended);
        await AssertDeniedAsync(suspended, course.Slug, paid.Id);

        var (expired, expiredId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(
            _factory.Services,
            expiredId,
            course.Id,
            EnrollmentStatus.Active,
            DateTimeOffset.UtcNow.AddDays(-1));
        await AssertDeniedAsync(expired, course.Slug, paid.Id);

        var (revoked, revokedId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, revokedId, course.Id, EnrollmentStatus.Revoked);
        await AssertDeniedAsync(revoked, course.Slug, paid.Id);
    }

    [Fact]
    public async Task Progress_ignores_draft_lessons_and_avoids_divide_by_zero()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.AddLessonAsync(
            _factory.Services,
            course.Sections.Single().Id,
            "مسودة",
            9,
            LessonStatus.Draft);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        foreach (var lesson in course.Sections.Single().Lessons.Where(item => item.Status == LessonStatus.Published))
        {
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{lesson.Id}/complete", null)).StatusCode);
        }

        var learning = await client.GetFromJsonAsync<StudentCourseLearningResponse>(
            $"/api/student/courses/{course.Slug}",
            JsonOptions);
        Assert.NotNull(learning);
        Assert.Equal(2, learning.TotalLessons);
        Assert.Equal(100, learning.ProgressPercent);
        Assert.True(learning.CourseCompleted);
        Assert.DoesNotContain(learning.Sections.SelectMany(section => section.Lessons), item => item.Title == "مسودة");
    }

    [Fact]
    public async Task Continue_learning_prefers_last_incomplete_watched_lesson()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var first = FirstLesson(course);
        var second = course.Sections.Single().Lessons.Single(item => item.SortOrder == 2);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/progress/{second.Id}", new UpdateLessonProgressRequest { WatchedSeconds = 40 })).StatusCode);

        var summary = await client.GetFromJsonAsync<DashboardSummaryResponse>("/api/dashboard/summary", JsonOptions);
        Assert.Equal(second.Id, summary!.ContinueLearning!.LessonId);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{second.Id}/complete", null)).StatusCode);
        summary = await client.GetFromJsonAsync<DashboardSummaryResponse>("/api/dashboard/summary", JsonOptions);
        Assert.Equal(first.Id, summary!.ContinueLearning!.LessonId);
    }

    [Fact]
    public async Task Student_can_read_create_and_update_own_progress()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lessonId = FirstLesson(course).Id;
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        var before = await client.GetFromJsonAsync<LessonProgressResponse>($"/api/progress/{lessonId}", JsonOptions);
        Assert.NotNull(before);
        Assert.Equal(0, before.WatchedSeconds);
        Assert.False(before.IsCompleted);

        var created = await client.PutAsJsonAsync(
            $"/api/progress/{lessonId}",
            new UpdateLessonProgressRequest { WatchedSeconds = 25 });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdBody = await created.Content.ReadFromJsonAsync<LessonProgressResponse>(JsonOptions);
        Assert.Equal(25, createdBody!.WatchedSeconds);
        Assert.NotNull(createdBody.LastWatchedAt);

        var updated = await client.PutAsJsonAsync(
            $"/api/progress/{lessonId}",
            new UpdateLessonProgressRequest { WatchedSeconds = 80 });
        var updatedBody = await updated.Content.ReadFromJsonAsync<LessonProgressResponse>(JsonOptions);
        Assert.Equal(80, updatedBody!.WatchedSeconds);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.LessonProgress.CountAsync(item => item.UserId == userId && item.LessonId == lessonId));
    }

    [Fact]
    public async Task Progress_updates_are_monotonic()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lessonId = FirstLesson(course).Id;
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        await client.PutAsJsonAsync($"/api/progress/{lessonId}", new UpdateLessonProgressRequest { WatchedSeconds = 100 });
        var delayed = await client.PutAsJsonAsync($"/api/progress/{lessonId}", new UpdateLessonProgressRequest { WatchedSeconds = 50 });
        var body = await delayed.Content.ReadFromJsonAsync<LessonProgressResponse>(JsonOptions);

        Assert.Equal(100, body!.WatchedSeconds);
    }

    [Fact]
    public async Task Complete_is_idempotent_and_does_not_duplicate_rows()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lessonId = FirstLesson(course).Id;
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{lessonId}/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/progress/{lessonId}/complete", null)).StatusCode);

        var progress = await client.GetFromJsonAsync<LessonProgressResponse>($"/api/progress/{lessonId}", JsonOptions);
        Assert.True(progress!.IsCompleted);
        Assert.NotNull(progress.CompletedAt);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.LessonProgress.CountAsync(item => item.UserId == userId && item.LessonId == lessonId));
    }

    [Fact]
    public async Task Unauthorized_student_cannot_update_progress()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lessonId = FirstLesson(course).Id;
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var update = await client.PutAsJsonAsync(
            $"/api/progress/{lessonId}",
            new UpdateLessonProgressRequest { WatchedSeconds = 20 });
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
    }

    [Fact]
    public async Task Anonymous_requests_cannot_create_progress()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var client = _factory.CreateAuthClient();
        var response = await client.PutAsJsonAsync(
            $"/api/progress/{FirstLesson(course).Id}",
            new UpdateLessonProgressRequest { WatchedSeconds = 10 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cross_course_lesson_attack_is_denied()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var courseA = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, courseA.Id);

        var lessonB = FirstLesson(courseB).Id;
        var response = await client.GetAsync($"/api/student/courses/{courseA.Slug}/lessons/{lessonB}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/student/courses/{courseB.Slug}")).StatusCode);
    }

    [Fact]
    public async Task Student_b_cannot_read_student_a_course_or_progress()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lessonId = FirstLesson(course).Id;
        var (studentA, userA) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userA, course.Id);
        await studentA.PutAsJsonAsync($"/api/progress/{lessonId}", new UpdateLessonProgressRequest { WatchedSeconds = 90 });

        var (studentB, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await studentB.GetAsync($"/api/student/courses/{course.Slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await studentB.GetAsync($"/api/progress/{lessonId}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await studentB.PutAsJsonAsync($"/api/progress/{lessonId}", new UpdateLessonProgressRequest { WatchedSeconds = 5 })).StatusCode);
    }

    [Fact]
    public async Task Playback_stays_protected_and_student_apis_do_not_leak_video_key()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.Single().Lessons.Single(item => !item.IsFreePreview);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        var learning = await client.GetAsync($"/api/student/courses/{course.Slug}");
        var lesson = await client.GetAsync($"/api/student/courses/{course.Slug}/lessons/{paid.Id}");
        var playback = await client.GetAsync($"/api/lessons/{paid.Id}/playback");
        Assert.Equal(HttpStatusCode.OK, learning.StatusCode);
        Assert.Equal(HttpStatusCode.OK, lesson.StatusCode);
        Assert.Equal(HttpStatusCode.OK, playback.StatusCode);

        foreach (var response in new[] { learning, lesson, playback })
        {
            var json = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("videoKey", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret-video-key", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("AdminNotes", json, StringComparison.OrdinalIgnoreCase);
        }

        var (stranger, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.GetAsync($"/api/lessons/{paid.Id}/playback")).StatusCode);
    }

    [Fact]
    public async Task Watched_seconds_are_clamped_to_duration_and_do_not_grant_access()
    {
        var (owner, ownerId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var lesson = FirstLesson(course);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, ownerId, course.Id);

        var update = await owner.PutAsJsonAsync(
            $"/api/progress/{lesson.Id}",
            new UpdateLessonProgressRequest { WatchedSeconds = 999_999 });
        var body = await update.Content.ReadFromJsonAsync<LessonProgressResponse>(JsonOptions);
        Assert.Equal(lesson.DurationSeconds, body!.WatchedSeconds);

        var (stranger, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await stranger.PutAsJsonAsync(
                $"/api/progress/{lesson.Id}",
                new UpdateLessonProgressRequest { WatchedSeconds = 999_999 })).StatusCode);
    }

    [Fact]
    public async Task Lesson_learning_returns_previous_and_next_neighbors()
    {
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var first = FirstLesson(course);
        var second = course.Sections.Single().Lessons.Single(item => item.SortOrder == 2);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        var lesson = await client.GetFromJsonAsync<StudentLessonLearningResponse>(
            $"/api/student/courses/{course.Slug}/lessons/{first.Id}",
            JsonOptions);
        Assert.NotNull(lesson);
        Assert.Null(lesson.PreviousLesson);
        Assert.Equal(second.Id, lesson.NextLesson!.Id);
    }

    [Fact]
    public async Task Profile_update_still_ignores_role_and_account_status()
    {
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var response = await client.PutAsJsonAsync("/api/profile", new
        {
            fullName = "اسم الطالب المحدّث",
            phoneNumber = AuthTestHelpers.UniquePhone(),
            whatsAppNumber = (string?)null,
            governorate = "بغداد",
            accountStatus = "Blocked",
            roles = new[] { "Admin" }
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserSummaryResponse>(JsonOptions);
        Assert.Equal("اسم الطالب المحدّث", body!.FullName);
        Assert.Equal("Active", body.AccountStatus);
        Assert.Contains("Student", body.Roles);
        Assert.DoesNotContain("Admin", body.Roles);
    }

    private async Task AssertDeniedAsync(HttpClient client, string slug, Guid lessonId)
    {
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/student/courses/{slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/student/courses/{slug}/lessons/{lessonId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/lessons/{lessonId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/lessons/{lessonId}/playback")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PutAsJsonAsync($"/api/progress/{lessonId}", new UpdateLessonProgressRequest { WatchedSeconds = 10 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/progress/{lessonId}/complete", null)).StatusCode);
    }

    private static Domain.Entities.Lesson FirstLesson(Domain.Entities.Course course) =>
        course.Sections.Single().Lessons.Single(item => item.SortOrder == 1);

    private async Task<List<Guid>> PublishedLessonIdsAsync(Guid courseId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Lessons
            .Where(item => item.CourseSection.CourseId == courseId && item.Status == LessonStatus.Published)
            .OrderBy(item => item.CourseSection.SortOrder)
            .ThenBy(item => item.SortOrder)
            .Select(item => item.Id)
            .ToListAsync();
    }
}
