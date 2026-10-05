using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Courses;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class VideoPlaybackSecurityTests : IClassFixture<VideoCaptureApiFactory>, IClassFixture<BunnyPlaybackApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly VideoCaptureApiFactory _captureFactory;
    private readonly BunnyPlaybackApiFactory _bunnyFactory;

    public VideoPlaybackSecurityTests(VideoCaptureApiFactory captureFactory, BunnyPlaybackApiFactory bunnyFactory)
    {
        _captureFactory = captureFactory;
        _bunnyFactory = bunnyFactory;
    }

    [Fact]
    public async Task Anonymous_protected_lesson_is_denied_and_does_not_call_playback_service()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var client = _captureFactory.CreateAuthClient();

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(capture.LessonIds);
    }

    [Fact]
    public async Task Student_without_enrollment_is_denied_and_does_not_call_playback_service()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_captureFactory, RoleNames.Student);

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(capture.LessonIds);
    }

    [Fact]
    public async Task Student_with_enrollment_calls_playback_service()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_captureFactory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_captureFactory.Services, userId, course.Id);

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");
        var body = await response.Content.ReadFromJsonAsync<LessonPlaybackResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(paid.Id, capture.LessonIds);
        Assert.False(body?.PlaybackUnavailable);
        Assert.Equal("iframe", body?.Kind);
    }

    [Fact]
    public async Task Suspended_enrollment_is_denied_playback()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_captureFactory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(
            _captureFactory.Services,
            userId,
            course.Id,
            status: EnrollmentStatus.Suspended);

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(capture.LessonIds);
    }

    [Fact]
    public async Task Expired_enrollment_is_denied_playback()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_captureFactory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(
            _captureFactory.Services,
            userId,
            course.Id,
            expiresAt: DateTimeOffset.UtcNow.AddDays(-1));

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(capture.LessonIds);
    }

    [Fact]
    public async Task Archived_course_with_active_enrollment_allows_playback()
    {
        var capture = _captureFactory.Services.GetRequiredService<CapturingVideoPlaybackService>();
        capture.Reset();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_captureFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_captureFactory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_captureFactory.Services, userId, course.Id);

        using (var scope = _captureFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = await db.Courses.FirstAsync(item => item.Id == course.Id);
            entity.Status = CourseStatus.Archived;
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync($"/api/lessons/{paid.Id}/playback");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(paid.Id, capture.LessonIds);
    }

    [Fact]
    public async Task Video_key_is_not_exposed_on_public_or_playback_contracts()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_bunnyFactory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_bunnyFactory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_bunnyFactory.Services, userId, course.Id);

        var publicDetail = await client.GetAsync($"/api/public/courses/{course.Slug}");
        var playback = await client.GetAsync($"/api/lessons/{paid.Id}/playback");
        var publicBody = await publicDetail.Content.ReadAsStringAsync();
        var playbackBody = await playback.Content.ReadAsStringAsync();
        var playbackJson = JsonSerializer.Deserialize<LessonPlaybackResponse>(playbackBody, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, publicDetail.StatusCode);
        Assert.Equal(HttpStatusCode.OK, playback.StatusCode);
        Assert.DoesNotContain("videoKey", publicBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("videoKey", playbackBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("unit-test-bunny-token-key", playbackBody, StringComparison.Ordinal);
        Assert.False(playbackJson?.PlaybackUnavailable);
        Assert.StartsWith("https://iframe.mediadelivery.net/embed/12345/", playbackJson?.PlaybackUrl);
        Assert.Contains("token=", playbackJson?.PlaybackUrl);
        Assert.Contains("expires=", playbackJson?.PlaybackUrl);
        Assert.Equal("BunnyStream", playbackJson?.Provider);
        Assert.True(playbackJson?.ExpiresAt > DateTimeOffset.UtcNow);
    }
}
