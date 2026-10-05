using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Purchases;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AdminPurchaseRequestTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public AdminPurchaseRequestTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_and_support_can_run_manual_payment_workflow_without_activation()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);

        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        Assert.Equal(HttpStatusCode.OK, (await support.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/contacted", new AdminPurchaseNoteRequest { Note = "تم الاتصال" })).StatusCode);

        var (admin, adminId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/awaiting-payment", new AdminAwaitingPaymentRequest { PaymentMethod = "زين كاش" })).StatusCode);

        var confirmed = await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/confirm-payment", new AdminConfirmPaymentRequest
        {
            PaymentMethod = "زين كاش",
            PaymentReference = "TX-1"
        });
        var detail = await confirmed.Content.ReadFromJsonAsync<AdminPurchaseRequestDetailResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        Assert.Equal("PaymentReceived", detail?.Status);
        Assert.NotNull(detail?.PaymentReceivedAt);
        Assert.Equal(adminId, detail?.ConfirmedBy);
        Assert.True(detail?.CanActivate);
        Assert.Contains(detail!.Timeline, item => item.ToStatus == "PaymentReceived");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(0, await db.CourseEnrollments.CountAsync(item => item.CourseId == course.Id));
        Assert.Equal(0, await db.ActivationCodes.CountAsync(item => item.PurchaseRequestId == request.Id));
        Assert.True(await db.AdminAuditLogs.AnyAsync(item =>
            item.Action == "PurchasePaymentConfirmed" && item.EntityId == request.Id));
    }

    [Fact]
    public async Task Invalid_transitions_and_terminal_states_are_rejected()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);

        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/confirm-payment", new AdminConfirmPaymentRequest { PaymentMethod = "زين كاش" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/awaiting-payment", new AdminAwaitingPaymentRequest())).StatusCode);

        var other = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest
        {
            CourseId = (await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services)).Id
        });
        var second = await other.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{second!.Id}/reject", new AdminPurchaseReasonRequest { Reason = "غير مكتمل" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{second.Id}/contacted", new AdminPurchaseNoteRequest())).StatusCode);

        var thirdCourse = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var thirdCreated = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = thirdCourse.Id });
        var third = await thirdCreated.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{third!.Id}/cancel", new AdminPurchaseReasonRequest { Reason = "طلب العميل" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{third.Id}/awaiting-payment", new AdminAwaitingPaymentRequest())).StatusCode);
    }

    [Fact]
    public async Task Content_manager_cannot_confirm_payment()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);

        var confirm = await manager.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/confirm-payment", new AdminConfirmPaymentRequest
        {
            PaymentMethod = "زين كاش"
        });
        Assert.Equal(HttpStatusCode.Forbidden, confirm.StatusCode);
    }

    [Fact]
    public async Task Rejected_request_allows_a_new_purchase_for_same_course()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var first = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await first.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/reject", new AdminPurchaseReasonRequest { Reason = "بيانات ناقصة" });

        var second = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }
}
