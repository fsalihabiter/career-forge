using CareerForge.Api.Content;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareerForge.Api.Tests;

public sealed class ContentImportTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Versioned_files_are_validated_and_imported_idempotently()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var importer = scope.ServiceProvider.GetRequiredService<ContentImportService>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var contentPath = Path.Combine(environment.ContentRootPath, "Content");

        var first = await importer.ImportAsync(contentPath);
        var second = await importer.ImportAsync(contentPath);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(new ContentImportReport(1, 3, 1, 1), first);
        Assert.Equal(first, second);
        var expectedStableIds = new[] { "middleware-order", "postgres-query-plan", "react-request-race" };
        var lessons = await db.Lessons.Include(x => x.Sections)
            .Where(x => expectedStableIds.Contains(x.StableId))
            .ToListAsync();
        Assert.Equal(3, lessons.Count);
        Assert.All(lessons, lesson =>
        {
            Assert.Equal(PublicationStatus.Published, lesson.Status);
            Assert.Equal(4, lesson.Sections.Count);
            Assert.NotEmpty(lesson.ObjectivesJson);
            Assert.NotNull(lesson.PublishedAt);
        });
        Assert.Contains(lessons, x => x.StableId == "middleware-order");
        Assert.Equal(1, await db.PatternGuides.CountAsync(x => x.StableId == "strategy-pattern"));
        Assert.Equal(1, await db.Questions.CountAsync(x => x.StableId == "api-idempotency"));
    }

    [Fact]
    public async Task Invalid_content_is_rejected_before_database_changes()
    {
        var root = Path.Combine(Path.GetTempPath(), $"careerforge-invalid-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "rubrics"));
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root, "rubrics", "invalid.json"),
                """
                {
                  "stableId": "invalid",
                  "version": 1,
                  "title": "Invalid",
                  "description": "",
                  "status": "draft",
                  "dimensions": [
                    { "key": "accuracy", "label": "Accuracy", "description": "", "weight": 90, "order": 1 }
                  ]
                }
                """);

            await using var scope = factory.Services.CreateAsyncScope();
            var importer = scope.ServiceProvider.GetRequiredService<ContentImportService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var before = await db.Rubrics.CountAsync();

            await Assert.ThrowsAsync<ContentValidationException>(() => importer.ImportAsync(root));

            Assert.Equal(before, await db.Rubrics.CountAsync());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Renamed_sections_are_replaced_atomically()
    {
        var root = Path.Combine(Path.GetTempPath(), $"careerforge-renamed-section-{Guid.NewGuid():N}");
        var lessonsPath = Path.Combine(root, "lessons");
        Directory.CreateDirectory(lessonsPath);
        var file = Path.Combine(lessonsPath, "rename-test.json");
        try
        {
            await File.WriteAllTextAsync(file, LessonJson("old-key"));
            await using var scope = factory.Services.CreateAsyncScope();
            var importer = scope.ServiceProvider.GetRequiredService<ContentImportService>();
            await importer.ImportAsync(root);

            await File.WriteAllTextAsync(file, LessonJson("new-key"));
            await importer.ImportAsync(root);

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var lesson = await db.Lessons.Include(x => x.Sections)
                .SingleAsync(x => x.StableId == "section-rename-test");
            Assert.Equal("new-key", Assert.Single(lesson.Sections).Key);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string LessonJson(string sectionKey)
        => $$"""
        {
          "stableId": "section-rename-test",
          "version": 1,
          "slug": "section-rename-test",
          "title": "Section rename test",
          "summary": "Importer update test",
          "technologySlug": "dotnet",
          "level": "intermediate",
          "estimatedMinutes": 5,
          "status": "draft",
          "objectives": ["Test"],
          "prerequisites": [],
          "category": null,
          "sections": [
            {
              "key": "{{sectionKey}}",
              "title": "Section",
              "order": 1,
              "bodyMarkdown": "Body",
              "codeLanguage": null,
              "codeSample": null
            }
          ]
        }
        """;
}
