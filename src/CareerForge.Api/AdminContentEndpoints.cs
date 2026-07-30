using System.Text.Json;
using System.Security.Claims;
using CareerForge.Api.Content;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api;

public static class AdminContentEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void Map(RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin/content")
            .RequireAuthorization(AppPolicies.ContentManagement);

        MapLearningContent<Lesson>(admin, "lessons", false);
        MapLearningContent<PatternGuide>(admin, "patterns", true);
        MapRubrics(admin);
        MapQuestions(admin);
        MapWorkflow(admin);
        MapVersioning(admin);
    }

    private static void MapLearningContent<T>(RouteGroupBuilder admin, string route, bool isPattern)
        where T : VersionedContent, new()
    {
        var group = admin.MapGroup($"/{route}");
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Set<T>().AsNoTracking().Include(x => x.Technology).Include(x => x.Sections)
                .OrderBy(x => x.StableId).ThenByDescending(x => x.Version)
                .Select(x => ToDefinition(x)).ToListAsync(ct)));

        group.MapGet("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Set<T>().AsNoTracking().Include(x => x.Technology).Include(x => x.Sections)
                .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            return item is null ? Results.NotFound() : Results.Ok(ToDefinition(item));
        });

        group.MapPost("/", async (LearningContentDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.Status != PublicationStatus.Draft)
                return Validation("Yeni içerik taslak durumunda oluşturulmalıdır.");
            var error = await ValidateLearningContent(request, isPattern, db, ct);
            if (error is not null) return Validation(error);
            if (await db.Set<T>().AnyAsync(x => x.StableId == request.StableId && x.Version == request.Version, ct))
                return Results.Conflict(new { detail = "Aynı stableId ve sürüme sahip içerik zaten var." });
            if (await db.Set<VersionedContent>().AnyAsync(x => x.Slug == request.Slug && x.Version == request.Version, ct))
                return Results.Conflict(new { detail = "Aynı slug ve sürüme sahip içerik zaten var." });

            var item = new T { Id = Guid.NewGuid(), StableId = request.StableId, Version = request.Version };
            await ApplyLearningContent(item, request, db, ct);
            db.Set<T>().Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/content/{route}/{item.StableId}/{item.Version}", ToDefinition(item));
        });

        group.MapPut("/{stableId}/{version:int}", async (
            string stableId, int version, LearningContentDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (stableId != request.StableId || version != request.Version)
                return Validation("Yol kimliği ve sürümü istek gövdesiyle eşleşmelidir.");
            var error = await ValidateLearningContent(request, isPattern, db, ct);
            if (error is not null) return Validation(error);
            var item = await db.Set<T>().Include(x => x.Sections)
                .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (item.Status != PublicationStatus.Draft)
                return Results.Conflict(new { detail = "Yalnızca taslak içerik düzenlenebilir; yeni sürüm oluşturun." });
            if (request.Status != item.Status)
                return Validation("Yayın durumu yalnızca durum geçişi eylemleriyle değiştirilebilir.");
            if (await db.Set<VersionedContent>().AnyAsync(
                    x => x.Id != item.Id && x.Slug == request.Slug && x.Version == request.Version, ct))
                return Results.Conflict(new { detail = "Aynı slug ve sürüme sahip içerik zaten var." });

            await ApplyLearningContent(item, request, db, ct);
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDefinition(item));
        });

        group.MapDelete("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Set<T>().SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (item.Status is PublicationStatus.InReview or PublicationStatus.Published)
                return Results.Conflict(new { detail = "İncelemedeki veya yayındaki içerik silinemez; önce arşivleyin." });
            db.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static void MapRubrics(RouteGroupBuilder admin)
    {
        var group = admin.MapGroup("/rubrics");
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Rubrics.AsNoTracking().Include(x => x.Dimensions)
                .OrderBy(x => x.StableId).ThenByDescending(x => x.Version)
                .Select(x => ToDefinition(x)).ToListAsync(ct)));
        group.MapGet("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Rubrics.AsNoTracking().Include(x => x.Dimensions)
                .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            return item is null ? Results.NotFound() : Results.Ok(ToDefinition(item));
        });
        group.MapPost("/", async (RubricDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.Status != PublicationStatus.Draft)
                return Validation("Yeni rubric taslak durumunda oluşturulmalıdır.");
            var error = ValidateRubric(request);
            if (error is not null) return Validation(error);
            if (await db.Rubrics.AnyAsync(x => x.StableId == request.StableId && x.Version == request.Version, ct))
                return Results.Conflict(new { detail = "Aynı stableId ve sürüme sahip rubric zaten var." });
            var item = new Rubric { Id = Guid.NewGuid(), StableId = request.StableId, Version = request.Version };
            ApplyRubric(item, request);
            db.Rubrics.Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/content/rubrics/{item.StableId}/{item.Version}", ToDefinition(item));
        });
        group.MapPut("/{stableId}/{version:int}", async (
            string stableId, int version, RubricDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (stableId != request.StableId || version != request.Version)
                return Validation("Yol kimliği ve sürümü istek gövdesiyle eşleşmelidir.");
            var error = ValidateRubric(request);
            if (error is not null) return Validation(error);
            var item = await db.Rubrics.Include(x => x.Dimensions)
                .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (item.Status != PublicationStatus.Draft)
                return Results.Conflict(new { detail = "Yalnızca taslak rubric düzenlenebilir; yeni sürüm oluşturun." });
            if (request.Status != item.Status)
                return Validation("Yayın durumu yalnızca durum geçişi eylemleriyle değiştirilebilir.");
            ApplyRubric(item, request);
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDefinition(item));
        });
        group.MapDelete("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Rubrics.SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (item.Status is PublicationStatus.InReview or PublicationStatus.Published)
                return Results.Conflict(new { detail = "İncelemedeki veya yayındaki rubric silinemez; önce arşivleyin." });
            if (await db.Questions.AnyAsync(x => x.RubricId == item.Id, ct))
                return Results.Conflict(new { detail = "Soruların kullandığı rubric silinemez." });
            db.Rubrics.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static void MapQuestions(RouteGroupBuilder admin)
    {
        var group = admin.MapGroup("/questions");
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Questions.AsNoTracking().Include(x => x.Skill).Include(x => x.Technology).Include(x => x.Rubric)
                .OrderBy(x => x.StableId).ThenByDescending(x => x.Version)
                .Select(x => ToDefinition(x)).ToListAsync(ct)));
        group.MapGet("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Questions.AsNoTracking().Include(x => x.Skill).Include(x => x.Technology).Include(x => x.Rubric)
                .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            return item is null ? Results.NotFound() : Results.Ok(ToDefinition(item));
        });
        group.MapPost("/", async (QuestionDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.Status != PublicationStatus.Draft)
                return Validation("Yeni soru taslak durumunda oluşturulmalıdır.");
            var references = await ResolveQuestionReferences(request, db, ct);
            if (references.Error is not null) return Validation(references.Error);
            if (await db.Questions.AnyAsync(x => x.StableId == request.StableId && x.Version == request.Version, ct))
                return Results.Conflict(new { detail = "Aynı stableId ve sürüme sahip soru zaten var." });
            var item = new Question { Id = Guid.NewGuid(), StableId = request.StableId, Version = request.Version };
            ApplyQuestion(item, request, references);
            db.Questions.Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/content/questions/{item.StableId}/{item.Version}", ToDefinition(item));
        });
        group.MapPut("/{stableId}/{version:int}", async (
            string stableId, int version, QuestionDefinition request, AppDbContext db, CancellationToken ct) =>
        {
            if (stableId != request.StableId || version != request.Version)
                return Validation("Yol kimliği ve sürümü istek gövdesiyle eşleşmelidir.");
            var references = await ResolveQuestionReferences(request, db, ct);
            if (references.Error is not null) return Validation(references.Error);
            var item = await db.Questions.SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (item.Status != PublicationStatus.Draft)
                return Results.Conflict(new { detail = "Yalnızca taslak soru düzenlenebilir; yeni sürüm oluşturun." });
            if (item.Status is PublicationStatus.InReview or PublicationStatus.Published)
                return Results.Conflict(new { detail = "İncelemedeki veya yayındaki soru silinemez; önce arşivleyin." });
            if (request.Status != item.Status)
                return Validation("Yayın durumu yalnızca durum geçişi eylemleriyle değiştirilebilir.");
            ApplyQuestion(item, request, references);
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDefinition(item));
        });
        group.MapDelete("/{stableId}/{version:int}", async (string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Questions.SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct);
            if (item is null) return Results.NotFound();
            if (await db.SessionQuestions.AnyAsync(x => x.QuestionId == item.Id, ct)
                || await db.ReviewItems.AnyAsync(x => x.QuestionId == item.Id, ct))
                return Results.Conflict(new { detail = "Kullanıcı verilerinin referans verdiği soru silinemez." });
            db.Questions.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static void MapWorkflow(RouteGroupBuilder admin)
    {
        admin.MapPost("/{kind}/{stableId}/{version:int}/transitions", async (
            string kind,
            string stableId,
            int version,
            ContentTransitionRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var entity = await FindContent(kind, stableId, version, db, ct);
            if (entity is null) return Results.NotFound();

            var current = StatusOf(entity);
            if (!AllowedTransition(current, request.TargetStatus))
                return Results.Conflict(new
                {
                    detail = $"{current} durumundan {request.TargetStatus} durumuna geçilemez."
                });
            if ((request.TargetStatus is PublicationStatus.Published or PublicationStatus.Archived)
                && !principal.IsInRole(AppRoles.Administrator))
                return Results.Forbid();
            if (request.TargetStatus == PublicationStatus.Published)
            {
                var publicationError = await ValidateForPublication(entity, db, ct);
                if (publicationError is not null) return Validation(publicationError);
            }

            SetStatus(entity, request.TargetStatus);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new ContentTransitionResponse(
                stableId, version, request.TargetStatus, PublishedAtOf(entity)));
        });
    }

    private static void MapVersioning(RouteGroupBuilder admin)
    {
        admin.MapPost("/{kind}/{stableId}/{version:int}/versions", async (
            string kind, string stableId, int version, AppDbContext db, CancellationToken ct) =>
        {
            var source = await FindVersionSource(kind, stableId, version, db, ct);
            if (source is null) return Results.NotFound();
            if (StatusOf(source) is not (PublicationStatus.Published or PublicationStatus.Archived))
                return Results.Conflict(new { detail = "Yeni sürüm yalnızca yayınlanmış veya arşivlenmiş içerikten üretilebilir." });
            if (await HasOpenVersion(kind, stableId, db, ct))
                return Results.Conflict(new { detail = "Bu içerik için zaten açık bir taslak veya inceleme sürümü var." });

            var nextVersion = await NextVersion(kind, stableId, db, ct);
            var clone = CloneVersion(source, nextVersion);
            db.Add(clone);
            await db.SaveChangesAsync(ct);
            return Results.Created(
                $"/api/admin/content/{kind}/{stableId}/{nextVersion}",
                new ContentVersionResponse(kind, stableId, version, nextVersion, PublicationStatus.Draft));
        });
    }

    private static async Task<object?> FindVersionSource(
        string kind, string stableId, int version, AppDbContext db, CancellationToken ct) => kind switch
    {
        "lessons" => await db.Lessons.AsNoTracking().Include(x => x.Sections)
            .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct),
        "patterns" => await db.PatternGuides.AsNoTracking().Include(x => x.Sections)
            .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct),
        "rubrics" => await db.Rubrics.AsNoTracking().Include(x => x.Dimensions)
            .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct),
        "questions" => await db.Questions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.StableId == stableId && x.Version == version, ct),
        _ => null
    };

    private static async Task<bool> HasOpenVersion(
        string kind, string stableId, AppDbContext db, CancellationToken ct) => kind switch
    {
        "lessons" => await db.Lessons.AnyAsync(x => x.StableId == stableId
            && (x.Status == PublicationStatus.Draft || x.Status == PublicationStatus.InReview), ct),
        "patterns" => await db.PatternGuides.AnyAsync(x => x.StableId == stableId
            && (x.Status == PublicationStatus.Draft || x.Status == PublicationStatus.InReview), ct),
        "rubrics" => await db.Rubrics.AnyAsync(x => x.StableId == stableId
            && (x.Status == PublicationStatus.Draft || x.Status == PublicationStatus.InReview), ct),
        "questions" => await db.Questions.AnyAsync(x => x.StableId == stableId
            && (x.Status == PublicationStatus.Draft || x.Status == PublicationStatus.InReview), ct),
        _ => false
    };

    private static async Task<int> NextVersion(
        string kind, string stableId, AppDbContext db, CancellationToken ct) => kind switch
    {
        "lessons" => await db.Lessons.Where(x => x.StableId == stableId).MaxAsync(x => x.Version, ct) + 1,
        "patterns" => await db.PatternGuides.Where(x => x.StableId == stableId).MaxAsync(x => x.Version, ct) + 1,
        "rubrics" => await db.Rubrics.Where(x => x.StableId == stableId).MaxAsync(x => x.Version, ct) + 1,
        "questions" => await db.Questions.Where(x => x.StableId == stableId).MaxAsync(x => x.Version, ct) + 1,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static object CloneVersion(object source, int version) => source switch
    {
        Lesson lesson => CloneLearningContent<Lesson>(lesson, version),
        PatternGuide pattern => CloneLearningContent<PatternGuide>(pattern, version),
        Rubric rubric => new Rubric
        {
            Id = Guid.NewGuid(), StableId = rubric.StableId, Version = version,
            Title = rubric.Title, Description = rubric.Description, Status = PublicationStatus.Draft,
            Dimensions = rubric.Dimensions.Select(x => new RubricDimension
            {
                Id = Guid.NewGuid(), Key = x.Key, Label = x.Label, Description = x.Description,
                Weight = x.Weight, Order = x.Order
            }).ToList()
        },
        Question question => new Question
        {
            Id = Guid.NewGuid(), StableId = question.StableId, Version = version,
            Prompt = question.Prompt, Type = question.Type, Level = question.Level,
            ModelAnswer = question.ModelAnswer, ExpectedSignalsJson = question.ExpectedSignalsJson,
            RedFlagsJson = question.RedFlagsJson, RubricJson = question.RubricJson,
            Status = PublicationStatus.Draft, RubricId = question.RubricId,
            SkillId = question.SkillId, TechnologyId = question.TechnologyId
        },
        _ => throw new ArgumentOutOfRangeException(nameof(source))
    };

    private static T CloneLearningContent<T>(VersionedContent source, int version)
        where T : VersionedContent, new()
    {
        var clone = new T
        {
            Id = Guid.NewGuid(), StableId = source.StableId, Version = version, Slug = source.Slug,
            Title = source.Title, Summary = source.Summary, TechnologyId = source.TechnologyId,
            Level = source.Level, EstimatedMinutes = source.EstimatedMinutes, Status = PublicationStatus.Draft,
            ObjectivesJson = source.ObjectivesJson, PrerequisitesJson = source.PrerequisitesJson
        };
        if (clone is PatternGuide clonePattern && source is PatternGuide sourcePattern)
            clonePattern.Category = sourcePattern.Category;
        foreach (var section in source.Sections)
            clone.Sections.Add(new ContentSection
            {
                Id = Guid.NewGuid(), Key = section.Key, Title = section.Title, Order = section.Order,
                BodyMarkdown = section.BodyMarkdown, CodeLanguage = section.CodeLanguage, CodeSample = section.CodeSample
            });
        return clone;
    }

    private static async Task<object?> FindContent(
        string kind, string stableId, int version, AppDbContext db, CancellationToken ct) => kind switch
    {
        "lessons" => await db.Lessons.SingleOrDefaultAsync(
            x => x.StableId == stableId && x.Version == version, ct),
        "patterns" => await db.PatternGuides.SingleOrDefaultAsync(
            x => x.StableId == stableId && x.Version == version, ct),
        "rubrics" => await db.Rubrics.SingleOrDefaultAsync(
            x => x.StableId == stableId && x.Version == version, ct),
        "questions" => await db.Questions.SingleOrDefaultAsync(
            x => x.StableId == stableId && x.Version == version, ct),
        _ => null
    };

    private static PublicationStatus StatusOf(object entity) => entity switch
    {
        VersionedContent content => content.Status,
        Rubric rubric => rubric.Status,
        Question question => question.Status,
        _ => throw new ArgumentOutOfRangeException(nameof(entity))
    };

    private static DateTimeOffset? PublishedAtOf(object entity) => entity switch
    {
        VersionedContent content => content.PublishedAt,
        Rubric rubric => rubric.PublishedAt,
        Question question => question.PublishedAt,
        _ => null
    };

    private static void SetStatus(object entity, PublicationStatus status)
    {
        var publishedAt = status == PublicationStatus.Published ? DateTimeOffset.UtcNow : (DateTimeOffset?)null;
        switch (entity)
        {
            case VersionedContent content:
                content.Status = status;
                content.PublishedAt = publishedAt;
                content.UpdatedAt = DateTimeOffset.UtcNow;
                break;
            case Rubric rubric:
                rubric.Status = status;
                rubric.PublishedAt = publishedAt;
                break;
            case Question question:
                question.Status = status;
                question.PublishedAt = publishedAt;
                break;
        }
    }

    private static bool AllowedTransition(PublicationStatus current, PublicationStatus target) =>
        (current, target) switch
        {
            (PublicationStatus.Draft, PublicationStatus.InReview) => true,
            (PublicationStatus.InReview, PublicationStatus.Draft) => true,
            (PublicationStatus.InReview, PublicationStatus.Published) => true,
            (PublicationStatus.Published, PublicationStatus.Archived) => true,
            (PublicationStatus.Archived, PublicationStatus.Draft) => true,
            _ => false
        };

    private static async Task<string?> ValidateForPublication(
        object entity, AppDbContext db, CancellationToken ct)
    {
        if (entity is Question question)
        {
            if (question.RubricId is null
                || !await db.Rubrics.AnyAsync(
                    x => x.Id == question.RubricId && x.Status == PublicationStatus.Published, ct))
                return "Soru yayınlanmadan önce bağlı rubric yayınlanmış olmalıdır.";
        }
        return null;
    }

    private static async Task<string?> ValidateLearningContent(
        LearningContentDefinition request, bool isPattern, AppDbContext db, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.StableId) || string.IsNullOrWhiteSpace(request.Slug)
            || string.IsNullOrWhiteSpace(request.Title))
            return "stableId, slug ve title zorunludur.";
        if (request.Version < 1 || request.EstimatedMinutes < 1) return "Sürüm ve tahmini süre pozitif olmalıdır.";
        if (request.Sections.Count == 0) return "En az bir bölüm zorunludur.";
        if (request.Sections.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Title)
                                      || string.IsNullOrWhiteSpace(x.BodyMarkdown)))
            return "Bölüm key, title ve bodyMarkdown alanları zorunludur.";
        if (request.Sections.Select(x => x.Key).Distinct(StringComparer.Ordinal).Count() != request.Sections.Count
            || request.Sections.Select(x => x.Order).Distinct().Count() != request.Sections.Count)
            return "Bölüm anahtarları ve sıraları benzersiz olmalıdır.";
        if (isPattern && string.IsNullOrWhiteSpace(request.Category)) return "Pattern kategorisi zorunludur.";
        if (request.TechnologySlug is not null
            && !await db.Technologies.AnyAsync(x => x.Slug == request.TechnologySlug, ct))
            return "Belirtilen teknoloji bulunamadı.";
        return null;
    }

    private static string? ValidateRubric(RubricDefinition request)
    {
        if (string.IsNullOrWhiteSpace(request.StableId) || string.IsNullOrWhiteSpace(request.Title))
            return "stableId ve title zorunludur.";
        if (request.Version < 1) return "Sürüm pozitif olmalıdır.";
        if (request.Dimensions.Count == 0 || request.Dimensions.Sum(x => x.Weight) != 100)
            return "Rubric boyut ağırlıkları toplamı 100 olmalıdır.";
        if (request.Dimensions.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Label)))
            return "Boyut key ve label alanları zorunludur.";
        if (request.Dimensions.Select(x => x.Key).Distinct(StringComparer.Ordinal).Count() != request.Dimensions.Count
            || request.Dimensions.Select(x => x.Order).Distinct().Count() != request.Dimensions.Count)
            return "Boyut anahtarları ve sıraları benzersiz olmalıdır.";
        return null;
    }

    private static async Task<QuestionReferences> ResolveQuestionReferences(
        QuestionDefinition request, AppDbContext db, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.StableId) || string.IsNullOrWhiteSpace(request.Prompt)
            || string.IsNullOrWhiteSpace(request.Type) || string.IsNullOrWhiteSpace(request.SkillSlug)
            || string.IsNullOrWhiteSpace(request.ModelAnswer))
            return new("Zorunlu soru alanları eksik.", null, null, null);
        if (request.Version < 1 || request.ExpectedSignals.Count == 0 || request.RedFlags.Count == 0)
            return new("Sürüm pozitif; güçlü sinyal ve kırmızı bayrak listeleri dolu olmalıdır.", null, null, null);
        var skill = await db.Skills.SingleOrDefaultAsync(x => x.Slug == request.SkillSlug, ct);
        var technology = request.TechnologySlug is null ? null
            : await db.Technologies.SingleOrDefaultAsync(x => x.Slug == request.TechnologySlug, ct);
        var rubric = await db.Rubrics.SingleOrDefaultAsync(
            x => x.StableId == request.RubricStableId && x.Version == request.RubricVersion, ct);
        if (skill is null || (request.TechnologySlug is not null && technology is null) || rubric is null)
            return new("Skill, teknoloji veya rubric referansı bulunamadı.", null, null, null);
        return new(null, skill, technology, rubric);
    }

    private static async Task ApplyLearningContent(
        VersionedContent item, LearningContentDefinition request, AppDbContext db, CancellationToken ct)
    {
        item.Slug = request.Slug.Trim();
        item.Title = request.Title.Trim();
        item.Summary = request.Summary.Trim();
        item.Technology = request.TechnologySlug is null ? null
            : await db.Technologies.SingleAsync(x => x.Slug == request.TechnologySlug, ct);
        item.Level = request.Level;
        item.EstimatedMinutes = request.EstimatedMinutes;
        item.Status = request.Status;
        item.ObjectivesJson = JsonSerializer.Serialize(request.Objectives, JsonOptions);
        item.PrerequisitesJson = JsonSerializer.Serialize(request.Prerequisites, JsonOptions);
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.PublishedAt = request.Status == PublicationStatus.Published ? item.PublishedAt ?? DateTimeOffset.UtcNow : null;
        if (item is PatternGuide pattern) pattern.Category = request.Category!.Trim();
        var requestedKeys = request.Sections.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
        db.ContentSections.RemoveRange(item.Sections.Where(x => !requestedKeys.Contains(x.Key)));
        foreach (var section in request.Sections)
        {
            var entity = item.Sections.SingleOrDefault(x => x.Key == section.Key);
            if (entity is null)
            {
                entity = new ContentSection { Id = Guid.NewGuid(), Key = section.Key };
                item.Sections.Add(entity);
            }
            entity.Title = section.Title;
            entity.Order = section.Order;
            entity.BodyMarkdown = section.BodyMarkdown;
            entity.CodeLanguage = section.CodeLanguage;
            entity.CodeSample = section.CodeSample;
        }
    }

    private static void ApplyRubric(Rubric item, RubricDefinition request)
    {
        item.Title = request.Title.Trim();
        item.Description = request.Description.Trim();
        item.Status = request.Status;
        item.PublishedAt = request.Status == PublicationStatus.Published ? item.PublishedAt ?? DateTimeOffset.UtcNow : null;
        var requestedKeys = request.Dimensions.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var removed in item.Dimensions.Where(x => !requestedKeys.Contains(x.Key)).ToArray())
            item.Dimensions.Remove(removed);
        foreach (var dimension in request.Dimensions)
        {
            var entity = item.Dimensions.SingleOrDefault(x => x.Key == dimension.Key);
            if (entity is null)
            {
                entity = new RubricDimension { Id = Guid.NewGuid(), Key = dimension.Key };
                item.Dimensions.Add(entity);
            }
            entity.Label = dimension.Label;
            entity.Description = dimension.Description;
            entity.Weight = dimension.Weight;
            entity.Order = dimension.Order;
        }
    }

    private static void ApplyQuestion(Question item, QuestionDefinition request, QuestionReferences references)
    {
        item.Prompt = request.Prompt.Trim();
        item.Type = request.Type.Trim();
        item.Level = request.Level;
        item.Skill = references.Skill!;
        item.Technology = references.Technology;
        item.Rubric = references.Rubric;
        item.ModelAnswer = request.ModelAnswer.Trim();
        item.ExpectedSignalsJson = JsonSerializer.Serialize(request.ExpectedSignals, JsonOptions);
        item.RedFlagsJson = JsonSerializer.Serialize(request.RedFlags, JsonOptions);
        item.Status = request.Status;
        item.PublishedAt = request.Status == PublicationStatus.Published ? item.PublishedAt ?? DateTimeOffset.UtcNow : null;
    }

    private static LearningContentDefinition ToDefinition(VersionedContent item) => new(
        item.StableId, item.Version, item.Slug, item.Title, item.Summary, item.Technology?.Slug,
        item.Level, item.EstimatedMinutes, item.Status,
        JsonSerializer.Deserialize<string[]>(item.ObjectivesJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<string[]>(item.PrerequisitesJson, JsonOptions) ?? [],
        (item as PatternGuide)?.Category,
        item.Sections.OrderBy(x => x.Order)
            .Select(x => new ContentSectionDefinition(x.Key, x.Title, x.Order, x.BodyMarkdown, x.CodeLanguage, x.CodeSample))
            .ToArray());

    private static RubricDefinition ToDefinition(Rubric item) => new(
        item.StableId, item.Version, item.Title, item.Description, item.Status,
        item.Dimensions.OrderBy(x => x.Order)
            .Select(x => new RubricDimensionDefinition(x.Key, x.Label, x.Description, x.Weight, x.Order)).ToArray());

    private static QuestionDefinition ToDefinition(Question item) => new(
        item.StableId, item.Version, item.Prompt, item.Type, item.Level, item.Skill.Slug,
        item.Technology?.Slug, item.Rubric!.StableId, item.Rubric.Version, item.ModelAnswer,
        JsonSerializer.Deserialize<string[]>(item.ExpectedSignalsJson, JsonOptions) ?? [],
        JsonSerializer.Deserialize<string[]>(item.RedFlagsJson, JsonOptions) ?? [], item.Status);

    private static IResult Validation(string error) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { ["content"] = [error] });

    private sealed record QuestionReferences(
        string? Error, Skill? Skill, Technology? Technology, Rubric? Rubric);

    public sealed record ContentTransitionRequest(PublicationStatus TargetStatus);
    public sealed record ContentTransitionResponse(
        string StableId, int Version, PublicationStatus Status, DateTimeOffset? PublishedAt);
    public sealed record ContentVersionResponse(
        string Kind, string StableId, int SourceVersion, int Version, PublicationStatus Status);
}
