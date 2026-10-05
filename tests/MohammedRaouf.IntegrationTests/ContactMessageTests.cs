using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Contact;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class ContactMessageTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public ContactMessageTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Valid_submit_admin_read_and_student_denied()
    {
        var anon = _factory.CreateAuthClient();
        var created = await anon.PostAsJsonAsync("/api/contact", ValidRequest());
        var body = await created.Content.ReadFromJsonAsync<CreateContactMessageResponse>(JsonOptions);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("New", body?.Status);

        var invalid = ValidRequest();
        invalid.Email = "bad";
        Assert.Equal(HttpStatusCode.BadRequest, (await anon.PostAsJsonAsync("/api/contact", invalid)).StatusCode);

        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync("/api/admin/contact-messages")).StatusCode);

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var list = await admin.GetFromJsonAsync<PagedResponse<AdminContactMessageSummaryResponse>>("/api/admin/contact-messages", JsonOptions);
        Assert.Contains(list!.Items, item => item.Id == body!.Id);

        var read = await admin.PostAsync($"/api/admin/contact-messages/{body!.Id}/mark-read", null);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
    }

    [Fact]
    public async Task Honeypot_does_not_persist_a_message()
    {
        var before = await CountAsync();
        var spam = ValidRequest();
        spam.Website = "https://bots.example";
        var response = await _factory.CreateAuthClient().PostAsJsonAsync("/api/contact", spam);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(before, await CountAsync());
    }

    [Fact]
    public async Task Content_manager_cannot_list_contact_messages()
    {
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/contact-messages")).StatusCode);
    }

    private static CreateContactMessageRequest ValidRequest() => new()
    {
        Name = "مراسل",
        Email = $"contact.{Guid.NewGuid():N}@example.test",
        Subject = "استفسار",
        Message = "هذه رسالة تواصل للاختبار."
    };

    private async Task<int> CountAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ContactMessages.CountAsync();
    }
}

[Collection("Postgres")]
public class ContactRateLimitingTests : IClassFixture<ContactRateLimitedAuthApiFactory>
{
    private readonly ContactRateLimitedAuthApiFactory _factory;

    public ContactRateLimitingTests(ContactRateLimitedAuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Contact_returns_429_after_the_configured_limit()
    {
        var client = _factory.CreateAuthClient();
        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            last = await client.PostAsJsonAsync("/api/contact", new CreateContactMessageRequest
            {
                Name = "مراسل",
                Email = $"limit.{attempt}.{Guid.NewGuid():N}@example.test",
                Subject = "موضوع",
                Message = "رسالة طويلة بما يكفي."
            });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}

[Collection("Postgres")]
public class ConsultationRateLimitingTests : IClassFixture<ConsultationRateLimitedAuthApiFactory>
{
    private readonly ConsultationRateLimitedAuthApiFactory _factory;

    public ConsultationRateLimitingTests(ConsultationRateLimitedAuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consultation_returns_429_after_the_configured_limit()
    {
        var client = _factory.CreateAuthClient();
        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            last = await client.PostAsJsonAsync("/api/consultations", new CreateConsultationRequest
            {
                FullName = "عميل",
                PhoneNumber = "07700000000",
                ConsultationType = "Business",
                Topic = "موضوع",
                Message = "تفاصيل الطلب",
                PreferredCommunicationMethod = "Phone"
            });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}
