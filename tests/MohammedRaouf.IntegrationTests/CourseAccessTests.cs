using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Courses;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class CourseAccessTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public CourseAccessTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_can_preview_published_free_lesson_on_published_course()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services, freePreview: true);
        var previewLesson = course.Sections.First().Lessons.OrderBy(lesson => lesson.SortOrder).First();
        var client = _factory.CreateAuthClient();

        var preview = await client.GetAsync($"/api/public/lessons/{previewLesson.Id}/preview");
        var body = await preview.Content.ReadFromJsonAsync<LessonPreviewResponse>(JsonOptions);
        var detail = await client.GetAsync($"/api/public/courses/{course.Slug}");
        var detailBody = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.True(body?.PreviewAvailable);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.DoesNotContain("secret-video-key", detailBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("videoKey", detailBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Anonymous_cannot_access_paid_lesson_metadata_or_playback()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var client = _factory.CreateAuthClient();

        var metadata = await client.GetAsync($"/api/lessons/{paid.Id}");
        var playback = await client.GetAsync($"/api/lessons/{paid.Id}/playback");
        var preview = await client.GetAsync($"/api/public/lessons/{paid.Id}/preview");

        Assert.Equal(HttpStatusCode.Unauthorized, metadata.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, playback.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, preview.StatusCode);
    }

    [Fact]
    public async Task Authenticated_student_without_enrollment_gets_forbidden_on_paid_lesson()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var metadata = await client.GetAsync($"/api/lessons/{paid.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, metadata.StatusCode);
    }

    [Fact]
    public async Task Student_b_cannot_read_lesson_from_course_owned_by_student_a()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (owner, ownerId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, ownerId, course.Id);
        var (intruder, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var allowed = await owner.GetAsync($"/api/lessons/{paid.Id}");
        var denied = await intruder.GetAsync($"/api/lessons/{paid.Id}");
        var playback = await owner.GetAsync($"/api/lessons/{paid.Id}/playback");
        var playbackBody = await playback.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        Assert.Equal(HttpStatusCode.OK, playback.StatusCode);
        Assert.DoesNotContain("secret-video-key", playbackBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("videoKey", playbackBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Active_lifetime_enrollment_grants_outline_even_if_course_is_archived()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, course.Id);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = await db.Courses.FirstAsync(item => item.Id == course.Id);
            Assert.NotNull(entity);
            entity.Status = CourseStatus.Archived;
            await db.SaveChangesAsync();
        }

        var outline = await client.GetAsync($"/api/courses/{course.Id}/outline");
        var publicDetail = await client.GetAsync($"/api/public/courses/{course.Slug}");
        var preview = await client.GetAsync($"/api/public/lessons/{course.Sections.First().Lessons.First().Id}/preview");

        Assert.Equal(HttpStatusCode.OK, outline.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, publicDetail.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, preview.StatusCode);
    }

    [Fact]
    public async Task Expired_or_suspended_enrollment_does_not_grant_access()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (expiredClient, expiredUser) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(
            _factory.Services,
            expiredUser,
            course.Id,
            expiresAt: DateTimeOffset.UtcNow.AddDays(-1));

        var (suspendedClient, suspendedUser) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        await CourseTestHelpers.SeedEnrollmentAsync(
            _factory.Services,
            suspendedUser,
            course.Id,
            status: EnrollmentStatus.Suspended);

        Assert.Equal(HttpStatusCode.Forbidden, (await expiredClient.GetAsync($"/api/lessons/{paid.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await suspendedClient.GetAsync($"/api/lessons/{paid.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await expiredClient.GetAsync($"/api/courses/{course.Id}/outline")).StatusCode);
    }

    [Fact]
    public async Task Unknown_lesson_returns_not_found()
    {
        var client = _factory.CreateAuthClient();
        var response = await client.GetAsync($"/api/lessons/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Public_course_detail_includes_curriculum_without_internal_fields()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var client = _factory.CreateAuthClient();
        var response = await client.GetFromJsonAsync<CourseDetailResponse>($"/api/public/courses/{course.Slug}", JsonOptions);

        Assert.NotNull(response);
        Assert.Equal("Lifetime", response.AccessType);
        Assert.True(response.LessonCount >= 2);
        Assert.Contains(response.Sections.SelectMany(section => section.Lessons), lesson => lesson.IsFreePreview);
        Assert.DoesNotContain(response.Sections.SelectMany(section => section.Lessons), lesson => lesson.Title.Contains("VideoKey", StringComparison.OrdinalIgnoreCase));
    }
}
