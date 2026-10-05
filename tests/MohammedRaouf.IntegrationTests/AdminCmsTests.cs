using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AdminCmsTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public AdminCmsTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Content_manager_and_admin_can_publish_article_but_support_and_student_cannot()
    {
        var payload = new SaveArticleRequest
        {
            Title = "مقال إداري",
            Slug = $"cms-{Guid.NewGuid():N}"[..18],
            Excerpt = "مقتطف واضح للمقال.",
            Content = "المحتوى الكامل للمقال بدون HTML."
        };

        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        var created = await manager.PostAsJsonAsync("/api/admin/articles", payload);
        var article = await created.Content.ReadFromJsonAsync<AdminArticleDetailResponse>(JsonOptions);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/articles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.PostAsJsonAsync("/api/admin/articles", payload)).StatusCode);

        var publicClient = _factory.CreateAuthClient();
        Assert.Equal(HttpStatusCode.NotFound, (await publicClient.GetAsync($"/api/public/articles/{article!.Slug}")).StatusCode);

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/articles/{article.Id}/publish", null)).StatusCode);

        var live = await publicClient.GetAsync($"/api/public/articles/{article.Slug}");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/articles/{article.Id}/archive", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publicClient.GetAsync($"/api/public/articles/{article.Slug}")).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AdminAuditLogs.AnyAsync(item => item.EntityId == article.Id && item.Action == "ArticlePublished"));
    }

    [Fact]
    public async Task Faq_permissions_and_reorder_require_complete_ids()
    {
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        var first = await (await manager.PostAsJsonAsync("/api/admin/faq", new SaveFaqRequest
        {
            Question = "سؤال واحد؟",
            Answer = "إجابة واحدة."
        })).Content.ReadFromJsonAsync<AdminFaqResponse>(JsonOptions);
        var second = await (await manager.PostAsJsonAsync("/api/admin/faq", new SaveFaqRequest
        {
            Question = "سؤال اثنان؟",
            Answer = "إجابة اثنان."
        })).Content.ReadFromJsonAsync<AdminFaqResponse>(JsonOptions);

        var reorder = await manager.PostAsJsonAsync("/api/admin/faq/reorder", new ReorderItemsRequest
        {
            Items =
            [
                new ReorderItemRequest { Id = first!.Id, SortOrder = 2 },
                new ReorderItemRequest { Id = second!.Id, SortOrder = 1 }
            ]
        });
        Assert.Equal(HttpStatusCode.OK, reorder.StatusCode);

        var incomplete = await manager.PostAsJsonAsync("/api/admin/faq/reorder", new ReorderItemsRequest
        {
            Items = [new ReorderItemRequest { Id = Guid.NewGuid(), SortOrder = 1 }]
        });
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/faq")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.PostAsync($"/api/admin/faq/{first.Id}/deactivate", null)).StatusCode);
    }

    [Fact]
    public async Task Content_settings_allowed_for_content_manager_payment_settings_admin_only()
    {
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        var content = await manager.PutAsJsonAsync("/api/admin/settings/content", new SaveSiteContentSettingsRequest
        {
            BrandName = "العيادة الإدارية",
            ConsultationInfo = "الاستشارة تُراجع قبل تأكيد الموعد."
        });
        Assert.Equal(HttpStatusCode.OK, content.StatusCode);

        var paymentDenied = await manager.PutAsJsonAsync("/api/admin/settings/payment", new SavePaymentSettingsRequest
        {
            TransferInstructions = "حوّل إلى الحساب المعتمد."
        });
        Assert.Equal(HttpStatusCode.Forbidden, paymentDenied.StatusCode);

        var settings = await manager.GetFromJsonAsync<AdminSiteSettingsResponse>("/api/admin/settings", JsonOptions);
        Assert.Equal("العيادة الإدارية", settings?.BrandName);
        Assert.Null(settings?.TransferInstructions);

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync("/api/admin/settings/payment", new SavePaymentSettingsRequest
        {
            TransferInstructions = "تعليمات التحويل العامة."
        })).StatusCode);

        var adminSettings = await admin.GetFromJsonAsync<AdminSiteSettingsResponse>("/api/admin/settings", JsonOptions);
        Assert.Equal("تعليمات التحويل العامة.", adminSettings?.TransferInstructions);
    }

    [Fact]
    public async Task Student_cannot_open_admin_dashboard()
    {
        var (student, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync("/api/admin/dashboard/summary")).StatusCode);
    }
}
