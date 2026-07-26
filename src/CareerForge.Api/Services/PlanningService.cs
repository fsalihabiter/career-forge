using System.Text.Json;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api.Services;

public sealed class PlanningService(AppDbContext db)
{
    public async Task<LearningPath> GenerateAsync(Guid userId, CancellationToken ct)
    {
        var userSkills = await db.UserSkills
            .Where(x => x.UserId == userId && x.IsActive)
            .Include(x => x.Skill)
            .ToListAsync(ct);

        var previous = await db.LearningPaths.Where(x => x.UserId == userId).ToListAsync(ct);
        db.LearningPaths.RemoveRange(previous);

        var ordered = userSkills
            .OrderBy(x => x.MeasuredLevel ?? x.SelfAssessedLevel ?? ProficiencyLevel.Beginner)
            .ThenBy(x => x.ConfidenceScore)
            .ToList();
        var path = new LearningPath
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SnapshotJson = JsonSerializer.Serialize(new
            {
                generatedAt = DateTimeOffset.UtcNow,
                skills = ordered.Select(x => new { x.SkillId, x.TechnologyId, x.TargetLevel })
            })
        };
        for (var index = 0; index < ordered.Count; index++)
        {
            var item = ordered[index];
            var current = item.MeasuredLevel ?? item.SelfAssessedLevel;
            path.Items.Add(new LearningPathItem
            {
                Id = Guid.NewGuid(),
                SkillId = item.SkillId,
                Order = index + 1,
                Title = item.Skill.Name,
                Reason = current is null
                    ? "Bu yetkinlik henüz ölçülmedi; tanılama ile başlangıç seviyeni belirle."
                    : $"{current} seviyesinden {item.TargetLevel} hedefine ilerle."
            });
        }
        db.Add(path);
        await db.SaveChangesAsync(ct);
        return path;
    }
}
