using System.Text.Json;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api.Services;

public sealed class SessionService(AppDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InterviewSession> StartAsync(Guid userId, SessionKind kind, int requestedCount, CancellationToken ct)
    {
        var count = Math.Clamp(requestedCount, 3, 15);
        var userSkills = await db.UserSkills.Where(x => x.UserId == userId && x.IsActive).Select(x => x.SkillId).ToListAsync(ct);
        var technologies = await db.UserTechnologies.Where(x => x.UserId == userId).Select(x => x.TechnologyId).ToListAsync(ct);
        var recentCutoff = DateTimeOffset.UtcNow.AddDays(-7);
        var recentQuestions = db.SessionQuestions
            .Where(x => x.Session.UserId == userId && x.Session.StartedAt > recentCutoff)
            .Select(x => new { x.QuestionId, x.Session.StartedAt });
        List<Guid> recentIds;
        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            var userQuestions = await db.SessionQuestions
                .Where(x => x.Session.UserId == userId)
                .Select(x => new { x.QuestionId, x.Session.StartedAt })
                .ToListAsync(ct);
            recentIds = userQuestions.Where(x => x.StartedAt > recentCutoff).Select(x => x.QuestionId).ToList();
        }
        else
        {
            recentIds = await recentQuestions.Select(x => x.QuestionId).ToListAsync(ct);
        }

        var query = db.Questions.AsNoTracking()
            .Where(x => x.Status == PublicationStatus.Published)
            .Where(x => userSkills.Count == 0 || userSkills.Contains(x.SkillId))
            .Where(x => x.TechnologyId == null || technologies.Contains(x.TechnologyId.Value));
        var candidates = await query
            .OrderBy(x => recentIds.Contains(x.Id))
            .ThenBy(x => x.Level)
            .Take(count * 2)
            .ToListAsync(ct);
        if (candidates.Count < count)
        {
            var selectedIds = candidates.Select(x => x.Id).ToArray();
            var supplementary = await db.Questions.AsNoTracking()
                .Where(x => x.Status == PublicationStatus.Published)
                .Where(x => !selectedIds.Contains(x.Id))
                .Where(x => x.TechnologyId == null || technologies.Contains(x.TechnologyId.Value))
                .OrderBy(x => recentIds.Contains(x.Id))
                .ThenBy(x => x.Level)
                .Take(count - candidates.Count)
                .ToListAsync(ct);
            candidates.AddRange(supplementary);
        }
        if (candidates.Count < count)
        {
            var selectedIds = candidates.Select(x => x.Id).ToArray();
            var supplementary = await db.Questions.AsNoTracking()
                .Where(x => x.Status == PublicationStatus.Published)
                .Where(x => !selectedIds.Contains(x.Id))
                .OrderBy(x => recentIds.Contains(x.Id))
                .ThenBy(x => x.Level)
                .Take(count - candidates.Count)
                .ToListAsync(ct);
            candidates.AddRange(supplementary);
        }
        var balanced = candidates
            .GroupBy(x => x.Type)
            .SelectMany(group => group.Take(2))
            .Take(count)
            .ToList();
        if (balanced.Count < count)
            balanced.AddRange(candidates.Where(x => balanced.All(y => y.Id != x.Id)).Take(count - balanced.Count));

        var session = new InterviewSession { Id = Guid.NewGuid(), UserId = userId, Kind = kind };
        for (var i = 0; i < balanced.Count; i++)
            session.Questions.Add(new SessionQuestion { SessionId = session.Id, QuestionId = balanced[i].Id, Order = i + 1 });
        db.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<object?> CompleteAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var session = await db.InterviewSessions
            .AsSplitQuery()
            .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Skill)
            .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Rubric).ThenInclude(x => x!.Dimensions)
            .SingleOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct);
        if (session is null) return null;

        session.Status = SessionStatus.Completed;
        session.CompletedAt = DateTimeOffset.UtcNow;
        var scored = session.Questions.Where(x => x.AnswerText != null && x.Question.Rubric != null).ToList();
        foreach (var item in scored)
            item.EvaluationJson = JsonSerializer.Serialize(
                RubricEvaluator.Evaluate(item.Question, item.AnswerText!),
                JsonOptions);

        foreach (var group in scored.GroupBy(x => x.Question.SkillId))
        {
            var sessionScore = group.Average(x => Evaluation(x).OverallScore);
            var userSkill = await db.UserSkills.FirstOrDefaultAsync(
                x => x.UserId == userId && x.SkillId == group.Key, ct);
            if (userSkill is null) continue;

            var history = await db.SkillAssessments
                .Where(x => x.UserSkillId == userSkill.Id && x.SessionId != session.Id)
                .ToListAsync(ct);
            var evidenceCount = group.Count();
            var previousEvidence = history.Sum(x => x.EvidenceCount);
            var totalEvidence = previousEvidence + evidenceCount;
            var rollingScore = (
                history.Sum(x => (double)x.SessionScore * x.EvidenceCount)
                + sessionScore * evidenceCount) / totalEvidence;
            var measured = LevelFor(rollingScore);
            var confidence = Math.Min(100, totalEvidence * 20);

            var assessment = await db.SkillAssessments.SingleOrDefaultAsync(
                x => x.SessionId == session.Id && x.UserSkillId == userSkill.Id, ct);
            assessment ??= new SkillAssessment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserSkillId = userSkill.Id,
                SessionId = session.Id
            };
            if (db.Entry(assessment).State == EntityState.Detached) db.SkillAssessments.Add(assessment);
            assessment.SessionScore = (decimal)Math.Round(sessionScore, 2);
            assessment.RollingScore = (decimal)Math.Round(rollingScore, 2);
            assessment.MeasuredLevel = measured;
            assessment.ConfidenceScore = confidence;
            assessment.EvidenceCount = evidenceCount;
            assessment.TotalEvidenceCount = totalEvidence;
            assessment.AssessedAt = session.CompletedAt.Value;

            userSkill.MeasuredLevel = measured;
            userSkill.ConfidenceScore = confidence;
            userSkill.LastAssessedAt = session.CompletedAt;
        }
        await db.SaveChangesAsync(ct);

        return new
        {
            session.Id,
            session.Kind,
            answered = scored.Count,
            total = session.Questions.Count,
            skills = scored.GroupBy(x => x.Question.Skill.Name).Select(group => new
            {
                skill = group.Key,
                score = Math.Round(group.Average(x => Evaluation(x).OverallScore), 1),
                confidence = Math.Min(100, group.Count() * 20),
                level = ScoreLabel(group.Average(x => Evaluation(x).OverallScore))
            })
        };
    }

    public static object ToDetail(InterviewSession session) => new
    {
        session.Id,
        kind = session.Kind.ToString().ToLowerInvariant(),
        status = session.Status.ToString().ToLowerInvariant(),
        session.StartedAt,
        questions = session.Questions.OrderBy(x => x.Order).Select(x => new
        {
            x.Question.Id,
            questionStableId = x.Question.StableId,
            questionVersion = x.Question.Version,
            x.Order,
            x.Question.Prompt,
            x.Question.Type,
            level = x.Question.Level.ToString().ToLowerInvariant(),
            skill = x.Question.Skill.Name,
            technology = x.Question.Technology == null ? null : x.Question.Technology.Name,
            answered = x.AnswerText != null,
            modelAnswer = session.Status == SessionStatus.Completed ? x.Question.ModelAnswer : null,
            signals = session.Status == SessionStatus.Completed
                ? JsonSerializer.Deserialize<string[]>(x.Question.ExpectedSignalsJson)
                : null,
            redFlags = session.Status == SessionStatus.Completed
                ? JsonSerializer.Deserialize<string[]>(x.Question.RedFlagsJson)
                : null,
            selfScore = session.Status == SessionStatus.Completed ? x.SelfScore : null,
            evaluation = session.Status == SessionStatus.Completed && x.EvaluationJson != null
                ? JsonSerializer.Deserialize<RubricEvaluation>(x.EvaluationJson, JsonOptions)
                : null
        })
    };

    private static RubricEvaluation Evaluation(SessionQuestion question) =>
        JsonSerializer.Deserialize<RubricEvaluation>(question.EvaluationJson!, JsonOptions)!;

    private static string ScoreLabel(double score) => score switch
    {
        < 25 => "beginner", < 45 => "basic", < 65 => "intermediate", < 85 => "advanced", _ => "expert"
    };

    private static ProficiencyLevel LevelFor(double score) => score switch
    {
        < 25 => ProficiencyLevel.Beginner,
        < 45 => ProficiencyLevel.Basic,
        < 65 => ProficiencyLevel.Intermediate,
        < 85 => ProficiencyLevel.Advanced,
        _ => ProficiencyLevel.Expert
    };
}
