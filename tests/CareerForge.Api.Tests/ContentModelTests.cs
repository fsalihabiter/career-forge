using CareerForge.Api.Data;
using CareerForge.Api.Models;
using CareerForge.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareerForge.Api.Tests;

public sealed class ContentModelTests(CareerForgeApiFactory factory)
    : IClassFixture<CareerForgeApiFactory>
{
    [Fact]
    public async Task Seeded_questions_reference_a_versioned_weighted_rubric()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var rubric = await db.Rubrics
            .Include(x => x.Dimensions)
            .Include(x => x.Questions)
            .SingleAsync(x => x.StableId == "default-technical-answer" && x.Version == 1);

        Assert.Equal(PublicationStatus.Published, rubric.Status);
        Assert.Equal(100, rubric.Dimensions.Sum(x => x.Weight));
        Assert.Equal(4, rubric.Dimensions.Count);
        Assert.NotEmpty(rubric.Questions);
        Assert.All(rubric.Questions, question =>
            Assert.Equal(PublicationStatus.Published, question.Status));
    }

    [Fact]
    public void Lessons_and_patterns_share_versioning_but_keep_distinct_types()
    {
        VersionedContent lesson = new Lesson
        {
            StableId = "middleware-order",
            Version = 2,
            Slug = "middleware-sirasi",
            Title = "Middleware sırası"
        };
        VersionedContent pattern = new PatternGuide
        {
            StableId = "outbox",
            Version = 1,
            Slug = "outbox",
            Title = "Outbox",
            Category = "Distributed system"
        };

        Assert.IsType<Lesson>(lesson);
        Assert.IsType<PatternGuide>(pattern);
        Assert.Equal(2, lesson.Version);
        Assert.Equal("outbox", pattern.StableId);
    }
}
