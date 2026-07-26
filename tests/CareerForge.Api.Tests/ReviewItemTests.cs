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

public sealed class ReviewItemTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Review_list_is_idempotent_filterable_and_account_scoped()
    {
        var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/review-items/")).StatusCode);

        Guid questionId;
        string skillSlug;
        string level;
        using (var scope = factory.Services.CreateScope())
        {
            var question = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
                .Questions.AsNoTracking().Include(x => x.Skill)
                .FirstAsync(x => x.Status == PublicationStatus.Published);
            questionId = question.Id;
            skillSlug = question.Skill.Slug;
            level = question.Level.ToString().ToLowerInvariant();
        }

        using var owner = await RegisterAsync($"review-{Guid.NewGuid():N}@careerforge.test");
        var first = await owner.PostAsync($"/api/review-items/{questionId}", null);
        var repeated = await owner.PostAsync($"/api/review-items/{questionId}", null);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);

        var firstItem = await first.Content.ReadFromJsonAsync<ReviewItemResponse>();
        var repeatedItem = await repeated.Content.ReadFromJsonAsync<ReviewItemResponse>();
        Assert.NotNull(firstItem);
        Assert.NotNull(repeatedItem);
        Assert.Equal(firstItem.Id, repeatedItem.Id);
        Assert.Equal(0, firstItem.IntervalDays);
        Assert.Equal(0, firstItem.RepetitionCount);

        var firstReviewResponse = await owner.PostAsJsonAsync(
            $"/api/review-items/{questionId}/reviews",
            new CompleteReviewRequest("good"));
        var firstReview = await firstReviewResponse.Content
            .ReadFromJsonAsync<ReviewItemResponse>();
        Assert.Equal(HttpStatusCode.OK, firstReviewResponse.StatusCode);
        Assert.NotNull(firstReview);
        Assert.Equal(1, firstReview.IntervalDays);
        Assert.Equal(1, firstReview.RepetitionCount);
        Assert.Equal(
            firstReview.LastReviewedAt!.Value.AddDays(1),
            firstReview.NextReviewAt);

        var secondReviewResponse = await owner.PostAsJsonAsync(
            $"/api/review-items/{questionId}/reviews",
            new CompleteReviewRequest("easy"));
        var secondReview = await secondReviewResponse.Content
            .ReadFromJsonAsync<ReviewItemResponse>();
        Assert.NotNull(secondReview);
        Assert.Equal(7, secondReview.IntervalDays);
        Assert.Equal(2, secondReview.RepetitionCount);
        Assert.Equal(
            secondReview.LastReviewedAt!.Value.AddDays(7),
            secondReview.NextReviewAt);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await owner.PostAsJsonAsync(
                $"/api/review-items/{questionId}/reviews",
                new CompleteReviewRequest("unknown"))).StatusCode);

        var bySkill = await owner.GetFromJsonAsync<List<ReviewItemResponse>>(
            $"/api/review-items/?skill={skillSlug}");
        var byLevel = await owner.GetFromJsonAsync<List<ReviewItemResponse>>(
            $"/api/review-items/?level={level}");
        var skillItem = Assert.Single(bySkill!);
        Assert.Single(byLevel!);
        Assert.Equal(questionId, skillItem.QuestionId);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await owner.GetAsync("/api/review-items/?level=unknown")).StatusCode);

        using var other = await RegisterAsync($"review-other-{Guid.NewGuid():N}@careerforge.test");
        Assert.Empty((await other.GetFromJsonAsync<List<ReviewItemResponse>>(
            "/api/review-items/"))!);
        Assert.Equal(HttpStatusCode.NotFound,
            (await other.DeleteAsync($"/api/review-items/{questionId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await other.PostAsJsonAsync(
                $"/api/review-items/{questionId}/reviews",
                new CompleteReviewRequest("good"))).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await owner.DeleteAsync($"/api/review-items/{questionId}")).StatusCode);
        Assert.Empty((await owner.GetFromJsonAsync<List<ReviewItemResponse>>(
            "/api/review-items/"))!);
    }

    private async Task<HttpClient> RegisterAsync(string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "IntegrationPass123", "Review User"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
