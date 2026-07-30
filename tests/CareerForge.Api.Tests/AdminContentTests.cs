using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareerForge.Api.Content;
using CareerForge.Api.Contracts;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CareerForge.Api.Tests;

public sealed class AdminContentTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public async Task Administrator_can_manage_and_validate_all_content_types()
    {
        using var client = factory.CreateClient();
        await AuthenticateAdministrator(client);
        var suffix = Guid.NewGuid().ToString("N");
        var rubricId = $"rubric-{suffix}";
        var lessonId = $"lesson-{suffix}";
        var patternId = $"pattern-{suffix}";
        var questionId = $"question-{suffix}";

        var invalidRubric = new RubricDefinition(
            rubricId, 1, "Rubric", "", PublicationStatus.Draft,
            [new("evidence", "Kanıt", "", 90, 1)]);
        var invalidResponse = await client.PostAsJsonAsync("/api/admin/content/rubrics/", invalidRubric);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        var rubric = invalidRubric with
        {
            Dimensions =
            [
                new RubricDimensionDefinition("evidence", "Kanıt", "", 60, 1),
                new RubricDimensionDefinition("clarity", "Netlik", "", 40, 2)
            ]
        };
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/admin/content/rubrics/", rubric)).StatusCode);

        var lesson = Content(lessonId, $"lesson-{suffix}", null);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/admin/content/lessons/", lesson)).StatusCode);

        var updatedLesson = lesson with { Title = "Güncellenmiş ders" };
        var updateResponse = await client.PutAsJsonAsync(
            $"/api/admin/content/lessons/{lessonId}/1", updatedLesson);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Güncellenmiş ders",
            (await updateResponse.Content.ReadFromJsonAsync<LearningContentDefinition>(JsonOptions))?.Title);

        var pattern = Content(patternId, $"pattern-{suffix}", "Dağıtık sistemler");
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/admin/content/patterns/", pattern)).StatusCode);

        var question = new QuestionDefinition(
            questionId, 1, "Nasıl çözersiniz?", "open-ended", ProficiencyLevel.Intermediate,
            "api-design", null, rubricId, 1, "Kanıtlı model cevap",
            ["ölçüm"], ["tahmin"], PublicationStatus.Draft);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/admin/content/questions/", question)).StatusCode);

        var listResponse = await client.GetAsync("/api/admin/content/lessons/");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains((await listResponse.Content.ReadFromJsonAsync<LearningContentDefinition[]>(JsonOptions))!,
            x => x.StableId == lessonId);

        Assert.Equal(HttpStatusCode.Conflict,
            (await client.DeleteAsync($"/api/admin/content/rubrics/{rubricId}/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/admin/content/questions/{questionId}/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/admin/content/rubrics/{rubricId}/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/admin/content/lessons/{lessonId}/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/admin/content/patterns/{patternId}/1")).StatusCode);
    }

    [Fact]
    public async Task Student_cannot_access_content_management()
    {
        using var client = factory.CreateClient();
        var registration = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest($"student-{Guid.NewGuid():N}@careerforge.test", "IntegrationPass123", "Student"));
        var auth = await registration.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await client.GetAsync("/api/admin/content/lessons/");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Editor_submits_content_and_only_administrator_can_publish_or_archive()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var stableId = $"workflow-{suffix}";
        var lesson = Content(stableId, $"workflow-{suffix}", null);
        using var editor = factory.CreateClient();
        await AuthenticateRole(editor, AppRoles.ContentEditor);

        Assert.Equal(HttpStatusCode.Created,
            (await editor.PostAsJsonAsync("/api/admin/content/lessons/", lesson)).StatusCode);
        var submit = await editor.PostAsJsonAsync(
            $"/api/admin/content/lessons/{stableId}/1/transitions",
            new { targetStatus = "inReview" });
        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);

        var directStatusEdit = await editor.PutAsJsonAsync(
            $"/api/admin/content/lessons/{stableId}/1",
            lesson with { Status = PublicationStatus.Published });
        Assert.Equal(HttpStatusCode.BadRequest, directStatusEdit.StatusCode);

        var forbiddenPublish = await editor.PostAsJsonAsync(
            $"/api/admin/content/lessons/{stableId}/1/transitions",
            new { targetStatus = "published" });
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenPublish.StatusCode);

        using var administrator = factory.CreateClient();
        await AuthenticateRole(administrator, AppRoles.Administrator);
        var publish = await administrator.PostAsJsonAsync(
            $"/api/admin/content/lessons/{stableId}/1/transitions",
            new { targetStatus = "published" });
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        Assert.NotNull((await publish.Content.ReadFromJsonAsync<TransitionResponse>(JsonOptions))?.PublishedAt);

        Assert.Equal(HttpStatusCode.Conflict,
            (await administrator.PostAsJsonAsync(
                $"/api/admin/content/lessons/{stableId}/1/transitions",
                new { targetStatus = "draft" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await administrator.PostAsJsonAsync(
                $"/api/admin/content/lessons/{stableId}/1/transitions",
                new { targetStatus = "archived" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await administrator.DeleteAsync($"/api/admin/content/lessons/{stableId}/1")).StatusCode);
    }

    private static LearningContentDefinition Content(string stableId, string slug, string? category) => new(
        stableId, 1, slug, "İçerik", "Özet", "dotnet", ProficiencyLevel.Intermediate, 15,
        PublicationStatus.Draft, ["Öğren"], [], category,
        [new ContentSectionDefinition("intro", "Giriş", 1, "İçerik", null, null)]);

    private async Task AuthenticateAdministrator(HttpClient client)
        => await AuthenticateRole(client, AppRoles.Administrator);

    private async Task AuthenticateRole(HttpClient client, string role)
    {
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@careerforge.test";
        const string password = "IntegrationPass123";
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { Id = Guid.NewGuid(), UserName = email, Email = email };
            Assert.True((await users.CreateAsync(user, password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        }
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }

    private sealed record TransitionResponse(
        string StableId, int Version, PublicationStatus Status, DateTimeOffset? PublishedAt);
}
