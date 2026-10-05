using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class RegisterLoginTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public RegisterLoginTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_valid_request_creates_student_without_enrollment()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        var body = await AuthTestHelpers.RegisterAsync(client, request);

        Assert.Equal(request.Email, body.Email);
        Assert.Contains(RoleNames.Student, body.Roles);
        Assert.Equal("Active", body.AccountStatus);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollments = await db.CourseEnrollments.CountAsync(enrollment => enrollment.UserId == body.Id);
        Assert.Equal(0, enrollments);
    }

    [Fact]
    public async Task Register_duplicate_email_is_rejected()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var duplicate = AuthTestHelpers.NewRegisterRequest(email: request.Email);
        var response = await client.PostAsJsonAsync("/api/auth/register", duplicate);
        var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("البريد الإلكتروني مستخدم مسبقاً.", problem?.Detail);
    }

    [Fact]
    public async Task Register_invalid_password_is_rejected()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest(password: "short");
        request.PasswordConfirmation = "short";

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_valid_credentials_sets_http_only_cookies()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var response = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password, rememberMe: true);
        var body = await response.Content.ReadFromJsonAsync<UserSummaryResponse>(AuthTestHelpers.JsonOptions);
        var cookies = AuthTestHelpers.SetCookies(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(request.Email, body.Email);
        Assert.DoesNotContain("accessToken", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(cookies, cookie => cookie.StartsWith("mr_access=", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cookies, cookie => cookie.StartsWith("mr_refresh=", StringComparison.OrdinalIgnoreCase));

        var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var meBody = await me.Content.ReadFromJsonAsync<UserSummaryResponse>(AuthTestHelpers.JsonOptions);
        Assert.Equal(request.Email, meBody?.Email);
    }

    [Fact]
    public async Task Login_wrong_password_fails_with_generic_message()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var response = await AuthTestHelpers.LoginAsync(client, request.Email, "WrongPass1");
        var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("البريد الإلكتروني أو كلمة المرور غير صحيحة.", problem?.Detail);
    }

    [Fact]
    public async Task Login_unknown_email_returns_generic_failure()
    {
        var client = _factory.CreateAuthClient();

        var response = await AuthTestHelpers.LoginAsync(client, AuthTestHelpers.UniqueEmail(), "Passw0rd1");
        var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("البريد الإلكتروني أو كلمة المرور غير صحيحة.", problem?.Detail);
    }

    [Fact]
    public async Task Login_suspended_user_is_denied()
    {
        await AssertInactiveLoginDenied(Domain.Enums.AccountStatus.Suspended);
    }

    [Fact]
    public async Task Login_blocked_user_is_denied()
    {
        await AssertInactiveLoginDenied(Domain.Enums.AccountStatus.Blocked);
    }

    private async Task AssertInactiveLoginDenied(Domain.Enums.AccountStatus status)
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        var created = await AuthTestHelpers.RegisterAsync(client, request);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.SingleAsync(item => item.Id == created.Id);
            user.AccountStatus = status;
            await db.SaveChangesAsync();
        }

        var response = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Suspended_account_cannot_use_an_existing_access_token()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        var created = await AuthTestHelpers.RegisterAsync(client, request);
        (await AuthTestHelpers.LoginAsync(client, request.Email, request.Password)).EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.SingleAsync(item => item.Id == created.Id);
            user.AccountStatus = Domain.Enums.AccountStatus.Suspended;
            await db.SaveChangesAsync();
        }

        var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
