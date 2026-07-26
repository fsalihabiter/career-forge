using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Tests.Infrastructure;

namespace CareerForge.Api.Tests;

public sealed class LessonProgressTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Progress_is_account_scoped_validated_and_restored_after_login()
    {
        var anonymous = factory.CreateClient();
        var anonymousResponse = await anonymous.GetAsync(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var email = $"progress-{Guid.NewGuid():N}@careerforge.test";
        const string password = "IntegrationPass123";
        using var firstDevice = await RegisterAsync(email, password);
        var initial = await firstDevice.GetFromJsonAsync<LessonProgressResponse>(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress");
        Assert.NotNull(initial);
        Assert.Equal(0, initial.CompletedSections);
        Assert.Equal(4, initial.TotalSections);
        Assert.False(initial.Completed);

        var savedResponse = await firstDevice.PutAsJsonAsync(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress",
            new UpdateLessonProgressRequest("kimlik", ["zincir"]));
        var saved = await savedResponse.Content.ReadFromJsonAsync<LessonProgressResponse>();
        Assert.Equal(HttpStatusCode.OK, savedResponse.StatusCode);
        Assert.NotNull(saved);
        Assert.Equal("kimlik", saved.LastSectionKey);
        Assert.Equal(["zincir"], saved.CompletedSectionKeys);

        using var secondDevice = await LoginAsync(email, password);
        var restored = await secondDevice.GetFromJsonAsync<LessonProgressResponse>(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress");
        Assert.NotNull(restored);
        Assert.Equal(saved.LastSectionKey, restored.LastSectionKey);
        Assert.Equal(saved.CompletedSectionKeys, restored.CompletedSectionKeys);

        var invalidResponse = await secondDevice.PutAsJsonAsync(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress",
            new UpdateLessonProgressRequest("not-a-section", ["zincir"]));
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        using var otherUser = await RegisterAsync(
            $"other-{Guid.NewGuid():N}@careerforge.test",
            password);
        var isolated = await otherUser.GetFromJsonAsync<LessonProgressResponse>(
            "/api/learning/lessons/aspnet-core-middleware-sirasi/progress");
        Assert.NotNull(isolated);
        Assert.Equal(0, isolated.CompletedSections);
    }

    private async Task<HttpClient> RegisterAsync(string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, "Progress User"));
        response.EnsureSuccessStatusCode();
        return await AuthenticateAsync(client, response);
    }

    private async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return await AuthenticateAsync(client, response);
    }

    private static async Task<HttpClient> AuthenticateAsync(
        HttpClient client,
        HttpResponseMessage response)
    {
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
