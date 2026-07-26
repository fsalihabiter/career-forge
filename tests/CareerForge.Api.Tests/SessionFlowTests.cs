using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Tests.Infrastructure;

namespace CareerForge.Api.Tests;

public sealed class SessionFlowTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Diagnostic_session_can_be_answered_completed_and_reviewed()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var startResponse = await client.PostAsJsonAsync(
            "/api/diagnostic-sessions/",
            new StartSessionRequest(3));
        var created = await startResponse.Content.ReadFromJsonAsync<SessionCreated>();
        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);
        Assert.NotNull(created);

        var activeSession = await client.GetFromJsonAsync<SessionDetail>(
            $"/api/diagnostic-sessions/{created.Id}");
        Assert.NotNull(activeSession);
        Assert.Equal("active", activeSession.Status);
        Assert.Equal(3, activeSession.Questions.Length);
        Assert.All(activeSession.Questions, question =>
        {
            Assert.False(question.Answered);
            Assert.Null(question.ModelAnswer);
            Assert.Null(question.Signals);
            Assert.Null(question.RedFlags);
        });

        foreach (var question in activeSession.Questions)
        {
            var answerResponse = await client.PostAsJsonAsync(
                $"/api/diagnostic-sessions/{created.Id}/answers/{question.Id}",
                new AnswerRequest($"Integration answer for {question.Id}", 70));
            Assert.Equal(HttpStatusCode.NoContent, answerResponse.StatusCode);
        }

        var completeResponse = await client.PostAsync(
            $"/api/diagnostic-sessions/{created.Id}/complete",
            null);
        var completion = await completeResponse.Content.ReadFromJsonAsync<CompletionResult>();
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        Assert.NotNull(completion);
        Assert.Equal(3, completion.Answered);
        Assert.Equal(3, completion.Total);

        var result = await client.GetFromJsonAsync<SessionDetail>(
            $"/api/diagnostic-sessions/{created.Id}/result");
        Assert.NotNull(result);
        Assert.Equal("completed", result.Status);
        Assert.All(result.Questions, question =>
        {
            Assert.True(question.Answered);
            Assert.False(string.IsNullOrWhiteSpace(question.ModelAnswer));
            Assert.NotNull(question.Signals);
            Assert.NotEmpty(question.Signals);
            Assert.NotNull(question.RedFlags);
            Assert.NotEmpty(question.RedFlags);
        });

        var editResponse = await client.PostAsJsonAsync(
            $"/api/diagnostic-sessions/{created.Id}/answers/{result.Questions[0].Id}",
            new AnswerRequest("Changed answer", 90));
        Assert.Equal(HttpStatusCode.Conflict, editResponse.StatusCode);
    }

    [Fact]
    public async Task Answer_endpoint_rejects_empty_text_and_invalid_score()
    {
        using var client = await CreateAuthenticatedClientAsync();
        var startResponse = await client.PostAsJsonAsync(
            "/api/interview-sessions/",
            new StartSessionRequest(3));
        var created = await startResponse.Content.ReadFromJsonAsync<SessionCreated>();
        Assert.NotNull(created);
        var session = await client.GetFromJsonAsync<SessionDetail>(
            $"/api/interview-sessions/{created.Id}");
        Assert.NotNull(session);

        var response = await client.PostAsJsonAsync(
            $"/api/interview-sessions/{created.Id}/answers/{session.Questions[0].Id}",
            new AnswerRequest(" ", 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(
                $"session-{Guid.NewGuid():N}@careerforge.test",
                "IntegrationPass123",
                "Session Test User"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    private sealed record SessionCreated(Guid Id);

    private sealed record CompletionResult(Guid Id, string Kind, int Answered, int Total);

    private sealed record SessionDetail(
        Guid Id,
        string Kind,
        string Status,
        DateTimeOffset StartedAt,
        SessionQuestion[] Questions);

    private sealed record SessionQuestion(
        Guid Id,
        int Order,
        string Prompt,
        string Type,
        string Level,
        string Skill,
        string? Technology,
        bool Answered,
        string? ModelAnswer,
        string[]? Signals,
        string[]? RedFlags);
}
