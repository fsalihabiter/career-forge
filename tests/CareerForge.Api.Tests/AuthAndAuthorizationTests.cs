using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CareerForge.Api.Contracts;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
        var registrationToken = new JwtSecurityTokenHandler().ReadJwtToken(registration.AccessToken);
        Assert.Contains(registrationToken.Claims,
            claim => claim.Type == ClaimTypes.Role && claim.Value == AppRoles.Student);

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
    public async Task Student_is_forbidden_from_administrator_policy()
    {
        using var client = factory.CreateClient();
        await AuthenticateAsync(client, "Student User");

        var response = await client.GetAsync("/api/admin/access");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_role_is_issued_in_token_and_grants_administrator_policy()
    {
        var email = UniqueEmail();
        const string password = "IntegrationPass123";
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var administrator = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email
            };
            Assert.True((await users.CreateAsync(administrator, password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(administrator, AppRoles.Administrator)).Succeeded);
        }

        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(login);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.AccessToken);
        Assert.Contains(token.Claims,
            claim => claim.Type == ClaimTypes.Role && claim.Value == AppRoles.Administrator);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.GetAsync("/api/admin/access");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var studentEndpoint = await client.GetAsync("/api/me/preparation-profile");
        Assert.Equal(HttpStatusCode.Forbidden, studentEndpoint.StatusCode);
    }

    [Fact]
    public async Task Role_seed_backfills_existing_users_without_overwriting_assigned_roles()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var email = UniqueEmail();
        var existingUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };
        Assert.True((await users.CreateAsync(existingUser, "IntegrationPass123")).Succeeded);

        await RoleSeed.ApplyAsync(roles, users);
        await RoleSeed.ApplyAsync(roles, users);

        Assert.Equal([AppRoles.Student], await users.GetRolesAsync(existingUser));
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
