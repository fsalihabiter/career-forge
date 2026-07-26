using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareerForge.Api.Tests;

public sealed class SkillProgressTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Completed_sessions_build_account_scoped_skill_history()
    {
        var email = $"skill-progress-{Guid.NewGuid():N}@careerforge.test";
        using var client = await RegisterAsync(email);
        var userSkillId = await AddTrackedSkillAsync(email);

        await CompleteSessionAsync(client);
        await CompleteSessionAsync(client);

        var history = await client.GetFromJsonAsync<SkillProgressHistoryResponse>(
            $"/api/me/skills/{userSkillId}/history");
        Assert.NotNull(history);
        Assert.Equal(userSkillId, history.UserSkillId);
        Assert.Equal("API tasarımı", history.Skill);
        Assert.Equal(2, history.History.Length);
        Assert.Equal([1, 2], history.History.Select(x => x.TotalEvidenceCount));
        Assert.Equal([20m, 40m], history.History.Select(x => x.ConfidenceScore));
        Assert.All(history.History, point =>
        {
            Assert.InRange(point.SessionScore, 0, 100);
            Assert.InRange(point.RollingScore, 0, 100);
            Assert.False(string.IsNullOrWhiteSpace(point.MeasuredLevel));
        });

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var current = await db.UserSkills.SingleAsync(x => x.Id == userSkillId);
            Assert.Equal(history.History[^1].MeasuredLevel, current.MeasuredLevel!.Value.ToString().ToLowerInvariant());
            Assert.Equal(history.History[^1].ConfidenceScore, current.ConfidenceScore);
        }

        using var otherUser = await RegisterAsync(
            $"other-progress-{Guid.NewGuid():N}@careerforge.test");
        var isolated = await otherUser.GetAsync($"/api/me/skills/{userSkillId}/history");
        Assert.Equal(HttpStatusCode.NotFound, isolated.StatusCode);
    }

    private async Task<Guid> AddTrackedSkillAsync(string email)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Email == email);
        var skill = await db.Skills.SingleAsync(x => x.Slug == "api-design");
        var userSkill = new UserSkill
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SkillId = skill.Id,
            SelfAssessedLevel = ProficiencyLevel.Intermediate,
            TargetLevel = ProficiencyLevel.Advanced,
            IsActive = true
        };
        db.UserSkills.Add(userSkill);
        await db.SaveChangesAsync();
        return userSkill.Id;
    }

    private static async Task CompleteSessionAsync(HttpClient client)
    {
        var start = await client.PostAsJsonAsync(
            "/api/diagnostic-sessions/",
            new StartSessionRequest(3));
        var created = await start.Content.ReadFromJsonAsync<SessionCreated>();
        Assert.NotNull(created);
        var session = await client.GetFromJsonAsync<SessionDetail>(
            $"/api/diagnostic-sessions/{created.Id}");
        Assert.NotNull(session);

        foreach (var question in session.Questions)
        {
            var response = await client.PostAsJsonAsync(
                $"/api/diagnostic-sessions/{created.Id}/answers/{question.Id}",
                new AnswerRequest(
                    "Idempotency key ve unique constraint kullanır, alternatifin riskini ölçerek transaction sınırında doğrularım.",
                    70));
            response.EnsureSuccessStatusCode();
        }

        var complete = await client.PostAsync(
            $"/api/diagnostic-sessions/{created.Id}/complete",
            null);
        complete.EnsureSuccessStatusCode();
    }

    private async Task<HttpClient> RegisterAsync(string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "IntegrationPass123", "Skill Progress User"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    private sealed record SessionCreated(Guid Id);
    private sealed record SessionDetail(SessionQuestion[] Questions);
    private sealed record SessionQuestion(Guid Id);
}
