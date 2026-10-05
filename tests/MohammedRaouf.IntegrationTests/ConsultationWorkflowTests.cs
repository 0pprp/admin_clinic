using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class ConsultationWorkflowTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public ConsultationWorkflowTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_can_submit_without_login_and_admin_can_run_happy_path()
    {
        var anon = _factory.CreateAuthClient();
        var created = await anon.PostAsJsonAsync("/api/consultations", ValidRequest());
        var body = await created.Content.ReadFromJsonAsync<CreateConsultationResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(body);
        Assert.StartsWith("CONS-", body.RequestNumber);
        Assert.Equal("New", body.Status);

        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync("/api/admin/consultations")).StatusCode);

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/admin/consultations/{body.Id}/contacted", new ConsultationNoteRequest())).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/admin/consultations/{body.Id}/complete", new ConsultationNoteRequest())).StatusCode);

        var scheduled = await admin.PostAsJsonAsync($"/api/admin/consultations/{body.Id}/schedule", new ScheduleConsultationRequest
        {
            ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(4)),
            ScheduledTime = new TimeOnly(10, 30)
        });
        var scheduledBody = await scheduled.Content.ReadFromJsonAsync<AdminConsultationDetailResponse>(JsonOptions);
        Assert.Equal(HttpStatusCode.OK, scheduled.StatusCode);
        Assert.Equal("Scheduled", scheduledBody?.Status);
        Assert.NotNull(scheduledBody?.ScheduledAt);

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/admin/consultations/{body.Id}/complete", new ConsultationNoteRequest())).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AdminAuditLogs.AnyAsync(item => item.EntityId == body.Id && item.Action == "ConsultationScheduled"));
    }

    [Fact]
    public async Task Invalid_email_and_past_date_are_rejected_and_honeypot_is_ignored()
    {
        var client = _factory.CreateAuthClient();
        var invalidEmail = ValidRequest();
        invalidEmail.Email = "not-an-email";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/consultations", invalidEmail)).StatusCode);

        var past = ValidRequest();
        past.PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-10));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/consultations", past)).StatusCode);

        var before = await CountConsultationsAsync();
        var spam = ValidRequest();
        spam.Website = "https://spam.example";
        var honeypot = await client.PostAsJsonAsync("/api/consultations", spam);
        Assert.True(honeypot.IsSuccessStatusCode);
        Assert.Equal(before, await CountConsultationsAsync());
    }

    [Fact]
    public async Task Support_can_manage_consultations_but_content_manager_cannot()
    {
        var anon = _factory.CreateAuthClient();
        var created = await (await anon.PostAsJsonAsync("/api/consultations", ValidRequest()))
            .Content.ReadFromJsonAsync<CreateConsultationResponse>(JsonOptions);

        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/consultations")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.PostAsJsonAsync($"/api/admin/consultations/{created!.Id}/contacted", new ConsultationNoteRequest())).StatusCode);

        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/consultations")).StatusCode);
    }

    [Fact]
    public async Task Logged_in_submit_binds_user_id()
    {
        var (student, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var created = await (await student.PostAsJsonAsync("/api/consultations", ValidRequest()))
            .Content.ReadFromJsonAsync<CreateConsultationResponse>(JsonOptions);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var row = await db.ConsultationRequests.SingleAsync(item => item.Id == created!.Id);
        Assert.Equal(userId, row.UserId);
    }

    private static CreateConsultationRequest ValidRequest() => new()
    {
        FullName = "عميل اختبار",
        PhoneNumber = "07701234567",
        ConsultationType = "Business",
        Topic = "تطوير العمل",
        Message = "أحتاج نقاشاً حول المرحلة التالية للمشروع.",
        PreferredCommunicationMethod = "WhatsApp",
        PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3))
    };

    private async Task<int> CountConsultationsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ConsultationRequests.CountAsync();
    }
}
