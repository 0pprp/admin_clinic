using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Purchases;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class PurchaseRequestTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public PurchaseRequestTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_can_create_request_for_published_course()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var response = await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest
        {
            CourseId = course.Id,
            CustomerNotes = "أرغب بالتسجيل."
        });
        var body = await response.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Pending", body?.Status);
        Assert.Equal(course.PriceIQD, body?.AmountIQD);
        Assert.StartsWith("MR-", body?.RequestNumber);
        Assert.Equal(course.Title, body?.CourseTitle);

        var mine = await client.GetFromJsonAsync<MohammedRaouf.Contracts.Public.PagedResponse<StudentPurchaseRequestSummaryResponse>>(
            "/api/purchase-requests?page=1&pageSize=10",
            JsonOptions);
        Assert.Contains(mine!.Items, item => item.Id == body!.Id);

        var detail = await client.GetFromJsonAsync<StudentPurchaseRequestDetailResponse>(
            $"/api/purchase-requests/{body!.Id}",
            JsonOptions);
        Assert.DoesNotContain("adminNotes", JsonSerializer.Serialize(detail), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(detail!.Timeline, item => item.Status == "Pending");
    }

    [Fact]
    public async Task Draft_and_archived_courses_cannot_be_purchased()
    {
        var draft = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services, courseStatus: CourseStatus.Draft);
        var archived = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services, courseStatus: CourseStatus.Archived);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = draft.Id })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = archived.Id })).StatusCode);
    }

    [Fact]
    public async Task Amount_is_snapshotted_when_course_price_changes()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var update = CourseTestHelpers.NewCourseRequest(slug: course.Slug, title: course.Title);
        update.PriceIQD = 300000;
        update.Description = course.Description;
        update.ShortDescription = course.ShortDescription;
        await admin.PutAsJsonAsync($"/api/admin/courses/{course.Id}", update);

        var detail = await student.GetFromJsonAsync<StudentPurchaseRequestDetailResponse>($"/api/purchase-requests/{request!.Id}", JsonOptions);
        Assert.Equal(150000, detail?.AmountIQD);
    }

    [Fact]
    public async Task Duplicate_open_request_returns_conflict()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id })).StatusCode);
        var duplicate = await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var problem = await duplicate.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("لديك طلب اشتراك قائم لهذه الدورة بالفعل.", problem?.Detail);
    }

    [Fact]
    public async Task Concurrent_duplicate_requests_create_only_one_open_row()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var firstTask = client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var secondTask = client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        await Task.WhenAll(firstTask, secondTask);

        var first = await firstTask;
        var second = await secondTask;
        var statuses = new[] { first.StatusCode, second.StatusCode };
        Assert.Contains(HttpStatusCode.Created, statuses);
        Assert.Contains(HttpStatusCode.Conflict, statuses);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var count = await db.PurchaseRequests.CountAsync(item => item.CourseId == course.Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Request_numbers_are_unique_under_parallel_creates()
    {
        var courses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services)));
        var clients = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student)));

        var responses = await Task.WhenAll(clients.Select((pair, index) =>
            pair.Client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = courses[index].Id })));

        var numbers = new List<string>();
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
            numbers.Add(body!.RequestNumber);
        }

        Assert.Equal(numbers.Count, numbers.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task Student_cannot_read_another_students_request()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (owner, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await owner.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        var (intruder, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);

        var denied = await intruder.GetAsync($"/api/purchase-requests/{request!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
    }

    [Fact]
    public async Task Student_cannot_confirm_payment()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);

        var confirm = await student.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/confirm-payment", new AdminConfirmPaymentRequest
        {
            PaymentMethod = "زين كاش"
        });
        Assert.Equal(HttpStatusCode.Forbidden, confirm.StatusCode);
    }

    [Fact]
    public async Task Suspended_account_cannot_create_purchase_request()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (client, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(item => item.Id == userId);
            user.AccountStatus = AccountStatus.Suspended;
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
