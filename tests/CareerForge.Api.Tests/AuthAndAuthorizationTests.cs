using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Tests.Infrastructure;

namespace CareerForge.Api.Tests;

public sealed class AuthAndAuthorizationTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Registered_user_can_login_and_receive_a_token()
    {
        using var client = factory.CreateClient();
        var email = UniqueEmail();
        const string password = "IntegrationPass123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Integration User"));
        var registration = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.NotNull(registration);
        Assert.False(string.IsNullOrWhiteSpace(registration.AccessToken));

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal("Integration User", login.DisplayName);
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_requests()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/me/preparation-profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_cannot_read_another_users_session()
    {
        using var owner = factory.CreateClient();
        using var otherUser = factory.CreateClient();
        await AuthenticateAsync(owner, "Session Owner");
        await AuthenticateAsync(otherUser, "Other User");

        var startResponse = await owner.PostAsJsonAsync(
            "/api/diagnostic-sessions/",
            new StartSessionRequest(3));
        var created = await startResponse.Content.ReadFromJsonAsync<SessionCreated>();
        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);
        Assert.NotNull(created);

        var ownerResponse = await owner.GetAsync($"/api/diagnostic-sessions/{created.Id}");
        var otherUserResponse = await otherUser.GetAsync($"/api/diagnostic-sessions/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, otherUserResponse.StatusCode);
    }

    private static async Task AuthenticateAsync(HttpClient client, string displayName)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(UniqueEmail(), "IntegrationPass123", displayName));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    private static string UniqueEmail() => $"integration-{Guid.NewGuid():N}@careerforge.test";

    private sealed record SessionCreated(Guid Id);
}
