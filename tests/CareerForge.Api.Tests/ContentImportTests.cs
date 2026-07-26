using CareerForge.Api.Content;
using CareerForge.Api.Data;
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
        Assert.Equal(new ContentImportReport(1, 1, 1, 1), first);
        Assert.Equal(first, second);
        Assert.Equal(1, await db.Lessons.CountAsync(x => x.StableId == "middleware-order"));
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
}
