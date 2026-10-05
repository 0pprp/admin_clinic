using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class PasswordResetTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public PasswordResetTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Reset_password_succeeds_and_revokes_old_sessions()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var login = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password, rememberMe: true);
        var refresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(login), "mr_refresh")!;
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        string encodedToken;
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(request.Email);
            Assert.NotNull(user);
            var token = await users.GeneratePasswordResetTokenAsync(user);
            encodedToken = AuthTestHelpers.EncodeResetToken(token);
        }

        var reset = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            Email = request.Email,
            Token = encodedToken,
            NewPassword = "N3wpassw0rd",
            ConfirmPassword = "N3wpassw0rd"
        });

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        using (var reuseClient = _factory.CreateAuthClient())
        using (var reuseRequest = AuthTestHelpers.CreateRefreshRequest(refresh))
        {
            var reused = await reuseClient.SendAsync(reuseRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var active = await db.RefreshTokens.CountAsync(token =>
                token.TokenHash == Infrastructure.Auth.TokenHashing.Hash(refresh) && token.RevokedAt == null);
            Assert.Equal(0, active);
        }

        var oldPasswordLogin = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);

        var newPasswordLogin = await AuthTestHelpers.LoginAsync(client, request.Email, "N3wpassw0rd");
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_always_returns_generic_message()
    {
        var client = _factory.CreateAuthClient();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            Email = AuthTestHelpers.UniqueEmail()
        });
        var body = await response.Content.ReadFromJsonAsync<MessageResponse>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("إذا كان البريد مسجلاً لدينا، فسيتم إرسال تعليمات استعادة كلمة المرور.", body?.Message);
    }
}
