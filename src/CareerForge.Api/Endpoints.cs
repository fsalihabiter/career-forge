using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CareerForge.Api.Contracts;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using CareerForge.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api;

public static class ClaimsExtensions
{
    public static Guid UserId(this ClaimsPrincipal principal)
        => Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());
}

public static class CatalogEndpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        api.MapGet("/technologies", async (AppDbContext db, CancellationToken ct) =>
            await db.Technologies.OrderBy(x => x.Category).ThenBy(x => x.Name)
                .Select(x => new CatalogTechnology(x.Id, x.Slug, x.Name, x.Category, x.Maturity.ToString().ToLower(), x.Accent))
                .ToListAsync(ct));
        api.MapGet("/skills", async (string? q, AppDbContext db, CancellationToken ct) =>
        {
            var query = db.Skills.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var pattern = $"%{q.Trim()}%";
                query = query.Where(x => EF.Functions.ILike(x.Name, pattern) || EF.Functions.ILike(x.Description, pattern));
            }
            return await query.OrderBy(x => x.Category).ThenBy(x => x.Name)
                .Select(x => new CatalogSkill(x.Id, x.Slug, x.Name, x.Category, x.Description)).ToListAsync(ct);
        });
        api.MapGet("/specializations", async (AppDbContext db, CancellationToken ct) =>
            await db.Specializations.Include(x => x.Skills).ThenInclude(x => x.Skill)
                .OrderBy(x => x.Name)
                .Select(x => new CatalogSpecialization(x.Id, x.Slug, x.Name, x.Description,
                    x.Skills.OrderByDescending(s => s.Weight)
                        .Select(s => (object)new { s.SkillId, s.Skill.Name, s.Required, s.Weight }).ToArray()))
                .ToListAsync(ct));
    }
}

public static class AuthEndpoints
{
    public static void Map(RouteGroupBuilder api, IConfiguration configuration)
    {
        var auth = api.MapGroup("/auth");
        auth.MapPost("/register", async (RegisterRequest request, UserManager<AppUser> users, AppDbContext db, TokenService tokens) =>
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 80)
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["displayName"] = ["Ad 1-80 karakter olmalıdır."] });
            var user = new AppUser { Id = Guid.NewGuid(), UserName = request.Email.Trim(), Email = request.Email.Trim() };
            var result = await users.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors.GroupBy(x => x.Code)
                    .ToDictionary(x => x.Key, x => x.Select(e => e.Description).ToArray()));
            var profile = new UserProfile { UserId = user.Id, TargetRole = request.DisplayName.Trim() };
            db.Add(profile);
            await db.SaveChangesAsync();
            var token = tokens.Create(user, request.DisplayName.Trim());
            return Results.Ok(new AuthResponse(token.Token, token.ExpiresAt, request.DisplayName.Trim(), false));
        });
        auth.MapPost("/login", async (LoginRequest request, UserManager<AppUser> users, AppDbContext db, TokenService tokens) =>
        {
            var user = await users.FindByEmailAsync(request.Email);
            if (user is null || !await users.CheckPasswordAsync(user, request.Password))
                return Results.Problem("E-posta veya parola geçersiz.", statusCode: 401);
            var profile = await db.UserProfiles.FindAsync(user.Id);
            var displayName = profile?.TargetRole ?? user.Email ?? "Kullanıcı";
            var token = tokens.Create(user, displayName);
            return Results.Ok(new AuthResponse(token.Token, token.ExpiresAt, displayName, profile?.OnboardingCompleted ?? false));
        });
    }
}

public static class LearningGuideEndpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var learning = api.MapGroup("/learning");

        learning.MapGet("/technologies", async (AppDbContext db, CancellationToken ct) =>
        {
            var technologies = await db.Technologies
                .Where(technology => db.Lessons.Any(lesson =>
                    lesson.TechnologyId == technology.Id
                    && lesson.Status == PublicationStatus.Published
                    && !db.Lessons.Any(newer =>
                        newer.StableId == lesson.StableId
                        && newer.Status == PublicationStatus.Published
                        && newer.Version > lesson.Version)))
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .Select(technology => new LearningTechnology(
                    technology.Id,
                    technology.Slug,
                    technology.Name,
                    technology.Category,
                    technology.Accent,
                    db.Lessons.Count(lesson =>
                        lesson.TechnologyId == technology.Id
                        && lesson.Status == PublicationStatus.Published
                        && !db.Lessons.Any(newer =>
                            newer.StableId == lesson.StableId
                            && newer.Status == PublicationStatus.Published
                            && newer.Version > lesson.Version))))
                .ToListAsync(ct);
            return Results.Ok(technologies);
        });

        learning.MapGet("/lessons", async (
            string? technology,
            string? level,
            AppDbContext db,
            CancellationToken ct) =>
        {
            ProficiencyLevel? parsedLevel = null;
            if (!string.IsNullOrWhiteSpace(level))
            {
                if (!Enum.TryParse<ProficiencyLevel>(level, ignoreCase: true, out var value)
                    || !Enum.IsDefined(value))
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["level"] = ["Geçerli bir yetkinlik seviyesi girilmelidir."]
                    });
                parsedLevel = value;
            }

            var query = LatestPublishedLessons(db);
            if (!string.IsNullOrWhiteSpace(technology))
                query = query.Where(x => x.Technology != null && x.Technology.Slug == technology.Trim());
            if (parsedLevel is not null)
                query = query.Where(x => x.Level == parsedLevel);

            var lessons = await query
                .Include(x => x.Technology)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Title)
                .ToListAsync(ct);
            return Results.Ok(lessons.Select(ToSummary));
        });

        learning.MapGet("/lessons/{slug}", async (string slug, AppDbContext db, CancellationToken ct) =>
        {
            var lesson = await LatestPublishedLessons(db)
                .Where(x => x.Slug == slug)
                .Include(x => x.Technology)
                .Include(x => x.Sections)
                .SingleOrDefaultAsync(ct);
            return lesson is null ? Results.NotFound() : Results.Ok(ToDetail(lesson));
        });
    }

    private static IQueryable<Lesson> LatestPublishedLessons(AppDbContext db)
        => db.Lessons.Where(lesson =>
            lesson.Status == PublicationStatus.Published
            && !db.Lessons.Any(newer =>
                newer.StableId == lesson.StableId
                && newer.Status == PublicationStatus.Published
                && newer.Version > lesson.Version));

    private static LessonSummary ToSummary(Lesson lesson)
        => new(
            lesson.StableId,
            lesson.Version,
            lesson.Slug,
            lesson.Title,
            lesson.Summary,
            lesson.Level.ToString().ToLowerInvariant(),
            lesson.EstimatedMinutes,
            ToTechnology(lesson.Technology));

    private static LessonDetail ToDetail(Lesson lesson)
        => new(
            lesson.StableId,
            lesson.Version,
            lesson.Slug,
            lesson.Title,
            lesson.Summary,
            lesson.Level.ToString().ToLowerInvariant(),
            lesson.EstimatedMinutes,
            ToTechnology(lesson.Technology),
            DeserializeArray(lesson.ObjectivesJson),
            DeserializeArray(lesson.PrerequisitesJson),
            lesson.Sections.OrderBy(x => x.Order)
                .Select(x => new LessonSection(x.Key, x.Title, x.Order, x.BodyMarkdown, x.CodeLanguage, x.CodeSample))
                .ToArray());

    private static CatalogTechnology? ToTechnology(Technology? technology)
        => technology is null
            ? null
            : new CatalogTechnology(
                technology.Id,
                technology.Slug,
                technology.Name,
                technology.Category,
                technology.Maturity.ToString().ToLowerInvariant(),
                technology.Accent);

    private static string[] DeserializeArray(string json)
        => JsonSerializer.Deserialize<string[]>(json) ?? [];
}

public static class ProfileEndpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var me = api.MapGroup("/me").RequireAuthorization();
        me.MapGet("/preparation-profile", async (ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var id = principal.UserId();
            var profile = await db.UserProfiles.SingleAsync(x => x.UserId == id, ct);
            var specializations = await db.UserSpecializations.Where(x => x.UserId == id).Select(x => x.SpecializationId).ToArrayAsync(ct);
            var technologies = await db.UserTechnologies.Where(x => x.UserId == id).Select(x => x.TechnologyId).ToArrayAsync(ct);
            return Results.Ok(new { profile, specializationIds = specializations, technologyIds = technologies });
        });
        me.MapPut("/preparation-profile", async (PreparationProfileRequest request, ClaimsPrincipal principal, AppDbContext db, PlanningService planner, CancellationToken ct) =>
        {
            if (request.WeeklyStudyMinutes is < 30 or > 2400 || request.ExperienceYears is < 0 or > 60)
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["profile"] = ["Deneyim veya haftalık çalışma süresi geçersiz."] });
            var id = principal.UserId();
            var profile = await db.UserProfiles.SingleAsync(x => x.UserId == id, ct);
            profile.Source = request.Source;
            profile.TargetRole = request.TargetRole.Trim();
            profile.TargetSeniority = request.TargetSeniority;
            profile.ExperienceYears = request.ExperienceYears;
            profile.QuestionLanguage = request.QuestionLanguage;
            profile.PreferredCodeLanguage = request.PreferredCodeLanguage;
            profile.WeeklyStudyMinutes = request.WeeklyStudyMinutes;
            profile.InterviewDate = request.InterviewDate;
            profile.OnboardingCompleted = true;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            db.UserSpecializations.RemoveRange(db.UserSpecializations.Where(x => x.UserId == id));
            db.UserTechnologies.RemoveRange(db.UserTechnologies.Where(x => x.UserId == id));
            var existingSkills = await db.UserSkills.Where(x => x.UserId == id).ToListAsync(ct);
            foreach (var existing in existingSkills) existing.IsActive = false;
            foreach (var specId in request.SpecializationIds.Distinct())
                db.Add(new UserSpecialization { UserId = id, SpecializationId = specId });
            foreach (var techId in request.TechnologyIds.Distinct())
                db.Add(new UserTechnology { UserId = id, TechnologyId = techId });
            foreach (var skill in request.Skills.DistinctBy(x => new { x.SkillId, x.TechnologyId }))
            {
                var existing = existingSkills.FirstOrDefault(x => x.SkillId == skill.SkillId && x.TechnologyId == skill.TechnologyId);
                if (existing is not null)
                {
                    existing.SelfAssessedLevel = skill.SelfAssessedLevel;
                    existing.TargetLevel = skill.TargetLevel;
                    existing.IsActive = true;
                }
                else
                {
                    db.Add(new UserSkill
                    {
                        Id = Guid.NewGuid(), UserId = id, SkillId = skill.SkillId, TechnologyId = skill.TechnologyId,
                        SelfAssessedLevel = skill.SelfAssessedLevel, TargetLevel = skill.TargetLevel, IsActive = true
                    });
                }
            }
            await db.SaveChangesAsync(ct);
            var path = await planner.GenerateAsync(id, ct);
            return Results.Ok(new { profile.OnboardingCompleted, learningPathId = path.Id });
        });
        me.MapGet("/skills", async (ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var id = principal.UserId();
            return Results.Ok(await db.UserSkills.Where(x => x.UserId == id && x.IsActive).Include(x => x.Skill).Include(x => x.Technology)
                .Select(x => new
                {
                    x.Id, x.SkillId, skill = x.Skill.Name, x.TechnologyId,
                    technology = x.Technology == null ? null : x.Technology.Name,
                    x.SelfAssessedLevel, x.MeasuredLevel, x.TargetLevel, x.ConfidenceScore, x.LastAssessedAt
                }).ToListAsync(ct));
        });
        api.MapPost("/learning-paths/generate", async (ClaimsPrincipal principal, PlanningService planner, CancellationToken ct) =>
            Results.Ok(await planner.GenerateAsync(principal.UserId(), ct))).RequireAuthorization();
        api.MapGet("/learning-paths/current", async (ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var id = principal.UserId();
            var path = await db.LearningPaths.Where(x => x.UserId == id).Include(x => x.Items)
                .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
            return path is null ? Results.NotFound() : Results.Ok(new
            {
                path.Id, path.CreatedAt,
                items = path.Items.OrderBy(x => x.Order).Select(x => new { x.Id, x.Title, x.Reason, x.Order, x.Completed })
            });
        }).RequireAuthorization();
    }
}

public static class SessionEndpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        MapKind(api, "diagnostic-sessions", SessionKind.Diagnostic);
        MapKind(api, "interview-sessions", SessionKind.Interview);
    }

    private static void MapKind(RouteGroupBuilder api, string route, SessionKind kind)
    {
        var group = api.MapGroup($"/{route}").RequireAuthorization();
        group.MapPost("/", async (StartSessionRequest request, ClaimsPrincipal principal, SessionService sessions, CancellationToken ct) =>
        {
            var session = await sessions.StartAsync(principal.UserId(), kind, request.QuestionCount, ct);
            return Results.Created($"/api/{route}/{session.Id}", new { session.Id });
        });
        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var session = await db.InterviewSessions
                .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Skill)
                .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Technology)
                .SingleOrDefaultAsync(x => x.Id == id && x.UserId == principal.UserId() && x.Kind == kind, ct);
            return session is null ? Results.NotFound() : Results.Ok(SessionService.ToDetail(session));
        });
        group.MapPost("/{id:guid}/answers/{questionId:guid}", async (Guid id, Guid questionId, AnswerRequest request, ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            if (request.SelfScore is < 0 or > 100 || string.IsNullOrWhiteSpace(request.AnswerText))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["answer"] = ["Cevap ve 0-100 öz değerlendirme puanı gereklidir."] });
            var item = await db.SessionQuestions.Include(x => x.Session)
                .SingleOrDefaultAsync(x => x.SessionId == id && x.QuestionId == questionId && x.Session.UserId == principal.UserId() && x.Session.Kind == kind, ct);
            if (item is null) return Results.NotFound();
            if (item.Session.Status == SessionStatus.Completed) return Results.Conflict(new { message = "Tamamlanan oturum değiştirilemez." });
            item.AnswerText = request.AnswerText.Trim();
            item.SelfScore = request.SelfScore;
            item.AnsweredAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        group.MapPost("/{id:guid}/complete", async (Guid id, ClaimsPrincipal principal, SessionService sessions, CancellationToken ct) =>
        {
            var result = await sessions.CompleteAsync(principal.UserId(), id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });
        group.MapGet("/{id:guid}/result", async (Guid id, ClaimsPrincipal principal, AppDbContext db, CancellationToken ct) =>
        {
            var session = await db.InterviewSessions
                .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Skill)
                .Include(x => x.Questions).ThenInclude(x => x.Question).ThenInclude(x => x.Technology)
                .SingleOrDefaultAsync(x => x.Id == id && x.UserId == principal.UserId() && x.Kind == kind && x.Status == SessionStatus.Completed, ct);
            return session is null ? Results.NotFound() : Results.Ok(SessionService.ToDetail(session));
        });
    }
}
