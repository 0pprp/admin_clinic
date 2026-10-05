using System.Net;
using System.Net.Http.Json;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class ProfileTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public ProfileTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_can_read_own_profile()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);
        (await AuthTestHelpers.LoginAsync(client, request.Email, request.Password)).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserSummaryResponse>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(request.Email, body.Email);
        Assert.Equal(request.FullName, body.FullName);
    }

    [Fact]
    public async Task Profile_update_ignores_account_status_and_roles()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);
        (await AuthTestHelpers.LoginAsync(client, request.Email, request.Password)).EnsureSuccessStatusCode();

        var response = await client.PutAsJsonAsync("/api/profile", new
        {
            fullName = "اسم محدّث",
            phoneNumber = AuthTestHelpers.UniquePhone(),
            whatsAppNumber = (string?)null,
            governorate = "البصرة",
            accountStatus = "Blocked",
            roles = new[] { "Admin" },
            email = "attacker@example.test"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserSummaryResponse>(AuthTestHelpers.JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("اسم محدّث", body.FullName);
        Assert.Equal("البصرة", body.Governorate);
        Assert.Equal("Active", body.AccountStatus);
        Assert.Contains("Student", body.Roles);
        Assert.DoesNotContain("Admin", body.Roles);
        Assert.Equal(request.Email, body.Email);
    }
}
