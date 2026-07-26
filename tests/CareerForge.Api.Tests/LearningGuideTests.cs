using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareerForge.Api.Tests;

public sealed class LearningGuideTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Guide_returns_only_latest_published_lessons_with_ordered_detail()
    {
        await SeedLessonsAsync();

        var technologies = await client.GetFromJsonAsync<LearningTechnology[]>("/api/learning/technologies");
        var lessons = await client.GetFromJsonAsync<LessonSummary[]>(
            "/api/learning/lessons?technology=dotnet&level=intermediate");
        var detail = await client.GetFromJsonAsync<LessonDetail>(
            "/api/learning/lessons/test-middleware");
        var draftResponse = await client.GetAsync("/api/learning/lessons/test-draft");
        var invalidLevelResponse = await client.GetAsync("/api/learning/lessons?level=unknown");

        var technology = Assert.Single(technologies!);
        Assert.Equal("dotnet", technology.Slug);
        Assert.Equal(1, technology.LessonCount);

        var lesson = Assert.Single(lessons!);
        Assert.Equal(2, lesson.Version);
        Assert.Equal("Güncel middleware", lesson.Title);
        Assert.Equal("dotnet", lesson.Technology?.Slug);

        Assert.NotNull(detail);
        Assert.Equal(["Pipeline sırasını açıklamak"], detail.Objectives);
        Assert.Equal(["Temel HTTP bilgisi"], detail.Prerequisites);
        Assert.Equal(["first", "second"], detail.Sections.Select(x => x.Key));
        Assert.Equal(HttpStatusCode.NotFound, draftResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidLevelResponse.StatusCode);
    }

    private async Task SeedLessonsAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var technology = await db.Technologies.SingleAsync(x => x.Slug == "dotnet");
        if (await db.Lessons.AnyAsync(x => x.StableId == "guide-test")) return;

        db.Lessons.AddRange(
            Lesson(technology, 1, "Eski middleware", "test-middleware", PublicationStatus.Published),
            Lesson(technology, 2, "Güncel middleware", "test-middleware", PublicationStatus.Published),
            Lesson(technology, 1, "Taslak ders", "test-draft", PublicationStatus.Draft, "draft-test"));
        await db.SaveChangesAsync();
    }

    private static Lesson Lesson(
        Technology technology,
        int version,
        string title,
        string slug,
        PublicationStatus status,
        string stableId = "guide-test")
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StableId = stableId,
            Version = version,
            Slug = slug,
            Title = title,
            Summary = "Test özeti",
            Technology = technology,
            Level = ProficiencyLevel.Intermediate,
            EstimatedMinutes = 10,
            Status = status,
            PublishedAt = status == PublicationStatus.Published ? DateTimeOffset.UtcNow : null,
            ObjectivesJson = JsonSerializer.Serialize(new[] { "Pipeline sırasını açıklamak" }),
            PrerequisitesJson = JsonSerializer.Serialize(new[] { "Temel HTTP bilgisi" })
        };
        lesson.Sections =
        [
            Section(lesson, "second", 2),
            Section(lesson, "first", 1)
        ];
        return lesson;
    }

    private static ContentSection Section(Lesson lesson, string key, int order)
        => new()
        {
            Id = Guid.NewGuid(),
            Content = lesson,
            Key = key,
            Title = key,
            Order = order,
            BodyMarkdown = $"{key} body"
        };
}
