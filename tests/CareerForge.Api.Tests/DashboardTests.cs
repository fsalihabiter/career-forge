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

public sealed class DashboardTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Dashboard_combines_next_work_weak_skill_and_last_result_per_account()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/me/dashboard")).StatusCode);

        var email = $"dashboard-{Guid.NewGuid():N}@careerforge.test";
        using var owner = await RegisterAsync(email);
        var started = await owner.PostAsJsonAsync(
            "/api/diagnostic-sessions/",
            new StartSessionRequest(3));
        var created = await started.Content.ReadFromJsonAsync<SessionCreated>();
        Assert.NotNull(created);
        var session = await owner.GetFromJsonAsync<SessionDetail>(
            $"/api/diagnostic-sessions/{created.Id}");
        Assert.NotNull(session);
        await AddTrackedSkillAsync(email, session.Questions[0].Id);

        var review = await owner.PostAsync(
            $"/api/review-items/{session.Questions[0].Id}",
            null);
        review.EnsureSuccessStatusCode();
        foreach (var question in session.Questions)
        {
            var answer = await owner.PostAsJsonAsync(
                $"/api/diagnostic-sessions/{created.Id}/answers/{question.Id}",
                new AnswerRequest(
                    "Idempotency key, unique constraint ve transaction sınırını gerekçesiyle birlikte uygularım.",
                    70));
            answer.EnsureSuccessStatusCode();
        }
        (await owner.PostAsync(
            $"/api/diagnostic-sessions/{created.Id}/complete",
            null)).EnsureSuccessStatusCode();

        var dashboard = await owner.GetFromJsonAsync<DashboardSummaryResponse>(
            "/api/me/dashboard");
        Assert.NotNull(dashboard);
        Assert.Equal("review", dashboard.NextWork.Kind);
        Assert.Equal(session.Questions[0].Prompt, dashboard.NextWork.Title);
        Assert.Equal(1, dashboard.DueReviewCount);
        Assert.NotNull(dashboard.WeakestSkill);
        Assert.NotNull(dashboard.LastResult);
        Assert.Equal(created.Id, dashboard.LastResult.SessionId);
        Assert.Equal("diagnostic", dashboard.LastResult.Kind);
        Assert.Equal(3, dashboard.LastResult.AnsweredQuestions);
        Assert.InRange(dashboard.LastResult.Score, 1, 100);

        using var other = await RegisterAsync(
            $"dashboard-other-{Guid.NewGuid():N}@careerforge.test");
        var isolated = await other.GetFromJsonAsync<DashboardSummaryResponse>(
            "/api/me/dashboard");
        Assert.NotNull(isolated);
        Assert.Equal(0, isolated.DueReviewCount);
        Assert.Null(isolated.WeakestSkill);
        Assert.Null(isolated.LastResult);
        Assert.Equal("diagnostic", isolated.NextWork.Kind);
    }

    private async Task<HttpClient> RegisterAsync(string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "IntegrationPass123", "Dashboard User"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    private async Task AddTrackedSkillAsync(string email, Guid questionId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Email == email);
        var question = await db.Questions.SingleAsync(x => x.Id == questionId);
        db.UserSkills.Add(new UserSkill
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SkillId = question.SkillId,
            SelfAssessedLevel = ProficiencyLevel.Intermediate,
            TargetLevel = ProficiencyLevel.Advanced,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private sealed record SessionCreated(Guid Id);
    private sealed record SessionDetail(SessionQuestion[] Questions);
    private sealed record SessionQuestion(Guid Id, string Prompt);
}
