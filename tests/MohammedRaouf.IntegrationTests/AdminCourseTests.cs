using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AdminCourseTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public AdminCourseTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_and_content_manager_can_create_course_but_support_and_student_cannot()
    {
        var request = CourseTestHelpers.NewCourseRequest();
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var created = await admin.PostAsJsonAsync("/api/admin/courses", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        var managerCreated = await manager.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest());
        Assert.Equal(HttpStatusCode.Created, managerCreated.StatusCode);

        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest())).StatusCode);
    }

    [Fact]
    public async Task Duplicate_slug_returns_conflict()
    {
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var request = CourseTestHelpers.NewCourseRequest(slug: $"dup-{Guid.NewGuid():N}"[..18]);
        Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/admin/courses", request)).StatusCode);
        var duplicate = await admin.PostAsJsonAsync("/api/admin/courses", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Publish_requires_section_and_published_lesson()
    {
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var created = await admin.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest());
        var course = await created.Content.ReadFromJsonAsync<AdminCourseDetailResponse>(JsonOptions);
        Assert.NotNull(course);

        var emptyPublish = await admin.PostAsync($"/api/admin/courses/{course.Id}/publish", null);
        Assert.Equal(HttpStatusCode.BadRequest, emptyPublish.StatusCode);

        var sectionResponse = await admin.PostAsJsonAsync($"/api/admin/courses/{course.Id}/sections", new SaveSectionRequest
        {
            Title = "القسم الأول"
        });
        var section = await sectionResponse.Content.ReadFromJsonAsync<AdminSectionResponse>(JsonOptions);
        Assert.NotNull(section);

        var stillEmpty = await admin.PostAsync($"/api/admin/courses/{course.Id}/publish", null);
        Assert.Equal(HttpStatusCode.BadRequest, stillEmpty.StatusCode);

        var lessonResponse = await admin.PostAsJsonAsync($"/api/admin/sections/{section.Id}/lessons", new SaveLessonRequest
        {
            Title = "الدرس الأول",
            DurationSeconds = 90
        });
        var lesson = await lessonResponse.Content.ReadFromJsonAsync<AdminLessonResponse>(JsonOptions);
        Assert.NotNull(lesson);

        var unpublishedLesson = await admin.PostAsync($"/api/admin/courses/{course.Id}/publish", null);
        Assert.Equal(HttpStatusCode.BadRequest, unpublishedLesson.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/lessons/{lesson.Id}/publish", null)).StatusCode);
        var published = await admin.PostAsync($"/api/admin/courses/{course.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);

        var unpublish = await admin.PostAsync($"/api/admin/courses/{course.Id}/unpublish", null);
        var unpublished = await unpublish.Content.ReadFromJsonAsync<AdminCourseDetailResponse>(JsonOptions);
        Assert.Equal("Draft", unpublished?.Status);
    }

    [Fact]
    public async Task Reorder_rejects_items_from_another_course_or_section()
    {
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var first = await CreateCourseWithTwoSectionsAsync(admin);
        var second = await CreateCourseWithTwoSectionsAsync(admin);

        var invalidSections = await admin.PostAsJsonAsync($"/api/admin/courses/{first.CourseId}/sections/reorder", new ReorderItemsRequest
        {
            Items =
            [
                new ReorderItemRequest { Id = first.SectionA, SortOrder = 1 },
                new ReorderItemRequest { Id = second.SectionA, SortOrder = 2 }
            ]
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidSections.StatusCode);

        var valid = await admin.PostAsJsonAsync($"/api/admin/courses/{first.CourseId}/sections/reorder", new ReorderItemsRequest
        {
            Items =
            [
                new ReorderItemRequest { Id = first.SectionA, SortOrder = 2 },
                new ReorderItemRequest { Id = first.SectionB, SortOrder = 1 }
            ]
        });
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);

        var validLessons = await admin.PostAsJsonAsync($"/api/admin/sections/{first.SectionA}/lessons/reorder", new ReorderItemsRequest
        {
            Items =
            [
                new ReorderItemRequest { Id = first.LessonA, SortOrder = 2 }
            ]
        });
        Assert.Equal(HttpStatusCode.OK, validLessons.StatusCode);

        var invalidLessons = await admin.PostAsJsonAsync($"/api/admin/sections/{first.SectionA}/lessons/reorder", new ReorderItemsRequest
        {
            Items =
            [
                new ReorderItemRequest { Id = first.LessonA, SortOrder = 1 },
                new ReorderItemRequest { Id = second.LessonA, SortOrder = 2 }
            ]
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidLessons.StatusCode);
    }

    [Fact]
    public async Task Admin_detail_can_include_video_key_but_public_payload_does_not()
    {
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var created = await admin.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest(slug: $"pub-{Guid.NewGuid():N}"[..18]));
        var course = await created.Content.ReadFromJsonAsync<AdminCourseDetailResponse>(JsonOptions);
        var section = await (await admin.PostAsJsonAsync($"/api/admin/courses/{course!.Id}/sections", new SaveSectionRequest { Title = "قسم" }))
            .Content.ReadFromJsonAsync<AdminSectionResponse>(JsonOptions);
        var lesson = await (await admin.PostAsJsonAsync($"/api/admin/sections/{section!.Id}/lessons", new SaveLessonRequest
        {
            Title = "درس",
            DurationSeconds = 30,
            VideoKey = "admin-only-object-key",
            VideoProvider = "LocalPlaceholder"
        })).Content.ReadFromJsonAsync<AdminLessonResponse>(JsonOptions);
        await admin.PostAsync($"/api/admin/lessons/{lesson!.Id}/publish", null);
        await admin.PostAsync($"/api/admin/courses/{course.Id}/publish", null);

        var adminDetail = await admin.GetFromJsonAsync<AdminCourseDetailResponse>($"/api/admin/courses/{course.Id}", JsonOptions);
        var publicBody = await (await admin.GetAsync($"/api/public/courses/{course.Slug}")).Content.ReadAsStringAsync();

        Assert.Equal("admin-only-object-key", adminDetail?.Sections.Single().Lessons.Single().VideoKey);
        Assert.DoesNotContain("admin-only-object-key", publicBody, StringComparison.Ordinal);
        Assert.DoesNotContain("videoKey", publicBody, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<(Guid CourseId, Guid SectionA, Guid SectionB, Guid LessonA)> CreateCourseWithTwoSectionsAsync(HttpClient admin)
    {
        var created = await admin.PostAsJsonAsync("/api/admin/courses", CourseTestHelpers.NewCourseRequest());
        var course = await created.Content.ReadFromJsonAsync<AdminCourseDetailResponse>(JsonOptions);
        var sectionA = await (await admin.PostAsJsonAsync($"/api/admin/courses/{course!.Id}/sections", new SaveSectionRequest { Title = "أ" }))
            .Content.ReadFromJsonAsync<AdminSectionResponse>(JsonOptions);
        var sectionB = await (await admin.PostAsJsonAsync($"/api/admin/courses/{course.Id}/sections", new SaveSectionRequest { Title = "ب" }))
            .Content.ReadFromJsonAsync<AdminSectionResponse>(JsonOptions);
        var lessonA = await (await admin.PostAsJsonAsync($"/api/admin/sections/{sectionA!.Id}/lessons", new SaveLessonRequest { Title = "درس أ", DurationSeconds = 10 }))
            .Content.ReadFromJsonAsync<AdminLessonResponse>(JsonOptions);
        await admin.PostAsJsonAsync($"/api/admin/sections/{sectionB!.Id}/lessons", new SaveLessonRequest { Title = "درس ب", DurationSeconds = 10 });
        return (course.Id, sectionA.Id, sectionB.Id, lessonA!.Id);
    }
}
