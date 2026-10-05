using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AdminPanelAuthorizationTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AdminPanelAuthorizationTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Support_can_reach_operations_but_not_content_users_or_audit()
    {
        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/dashboard/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/purchase-requests")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/students")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/activation-codes")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.GetAsync("/api/admin/consultations")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/courses")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/articles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await support.GetAsync("/api/admin/audit-logs")).StatusCode);
    }

    [Fact]
    public async Task Content_manager_can_reach_courses_and_articles_but_not_payments()
    {
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync("/api/admin/dashboard/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync("/api/admin/courses")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync("/api/admin/articles")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.GetAsync("/api/admin/faq")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/purchase-requests")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/students")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/activation-codes")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.GetAsync("/api/admin/users")).StatusCode);
    }

    [Fact]
    public async Task Admin_dashboard_returns_real_counts()
    {
        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var summary = await admin.GetFromJsonAsync<AdminDashboardSummaryResponse>("/api/admin/dashboard/summary");
        Assert.NotNull(summary);
        Assert.True(summary.ActiveStudents >= 0);
        Assert.True(summary.PublishedCourses >= 0);
        Assert.NotNull(summary.RecentActivity);
    }
}

[Collection("Postgres")]
public class AdminAccessPolicyTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AdminAccessPolicyTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Staff_roles_satisfy_access_admin_panel()
    {
        Assert.True((await AuthorizeAsync(AuthorizationPolicies.AccessAdminPanel, RoleNames.Admin)).Succeeded);
        Assert.True((await AuthorizeAsync(AuthorizationPolicies.AccessAdminPanel, RoleNames.Support)).Succeeded);
        Assert.True((await AuthorizeAsync(AuthorizationPolicies.AccessAdminPanel, RoleNames.ContentManager)).Succeeded);
        Assert.False((await AuthorizeAsync(AuthorizationPolicies.AccessAdminPanel, RoleNames.Student)).Succeeded);
        Assert.False((await AuthorizeAsync(AuthorizationPolicies.ManageBusinessSettings, RoleNames.ContentManager)).Succeeded);
        Assert.True((await AuthorizeAsync(AuthorizationPolicies.ManageBusinessSettings, RoleNames.Admin)).Succeeded);
    }

    private async Task<Microsoft.AspNetCore.Authorization.AuthorizationResult> AuthorizeAsync(string policy, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)],
            authenticationType: "Test"));
        return await authorization.AuthorizeAsync(user, resource: null, policy);
    }
}
