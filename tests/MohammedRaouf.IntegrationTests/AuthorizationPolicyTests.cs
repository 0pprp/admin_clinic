using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AuthorizationPolicyTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AuthorizationPolicyTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_cannot_satisfy_RequireAdmin()
    {
        var result = await AuthorizeAsync(AuthorizationPolicies.RequireAdmin, RoleNames.Student);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Admin_satisfies_RequireAdmin()
    {
        var result = await AuthorizeAsync(AuthorizationPolicies.RequireAdmin, RoleNames.Admin);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Support_satisfies_ManagePayments()
    {
        var result = await AuthorizeAsync(AuthorizationPolicies.ManagePayments, RoleNames.Support);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Support_satisfies_ManageActivations()
    {
        var result = await AuthorizeAsync(AuthorizationPolicies.ManageActivations, RoleNames.Support);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ContentManager_satisfies_ManageCourses()
    {
        var result = await AuthorizeAsync(AuthorizationPolicies.ManageCourses, RoleNames.ContentManager);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Student_satisfies_none_of_the_admin_policies()
    {
        foreach (var policy in AuthorizationPolicies.RoleMap.Keys)
        {
            var result = await AuthorizeAsync(policy, RoleNames.Student);
            Assert.False(result.Succeeded);
        }
    }

    private async Task<AuthorizationResult> AuthorizeAsync(string policy, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)],
            authenticationType: "Test"));
        return await authorization.AuthorizeAsync(user, resource: null, policy);
    }
}
