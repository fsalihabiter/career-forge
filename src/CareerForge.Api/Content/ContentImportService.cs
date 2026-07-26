using System.Text.Json;
using System.Text.Json.Serialization;
using CareerForge.Api.Data;
using CareerForge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api.Content;

public sealed class ContentImportService(AppDbContext db, ILogger<ContentImportService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<ContentImportReport> ImportAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(rootPath))
            throw new ContentValidationException($"İçerik dizini bulunamadı: {rootPath}");

        var rubrics = await ReadAsync<RubricDefinition>(rootPath, "rubrics", cancellationToken);
        var lessons = await ReadAsync<LearningContentDefinition>(rootPath, "lessons", cancellationToken);
        var patterns = await ReadAsync<LearningContentDefinition>(rootPath, "patterns", cancellationToken);
        var questions = await ReadAsync<QuestionDefinition>(rootPath, "questions", cancellationToken);

        Validate(rubrics, lessons, patterns, questions);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await UpsertRubricsAsync(rubrics, cancellationToken);
        await UpsertContentAsync<Lesson>(lessons, cancellationToken);
        await UpsertContentAsync<PatternGuide>(patterns, cancellationToken);
        await UpsertQuestionsAsync(questions, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();

        var report = new ContentImportReport(rubrics.Count, lessons.Count, patterns.Count, questions.Count);
        logger.LogInformation(
            "Git içeriği yüklendi: {Rubrics} rubric, {Lessons} ders, {Patterns} pattern, {Questions} soru.",
            report.Rubrics, report.Lessons, report.Patterns, report.Questions);
        return report;
    }

    private static async Task<List<T>> ReadAsync<T>(string rootPath, string directory, CancellationToken cancellationToken)
    {
        var path = Path.Combine(rootPath, directory);
        if (!Directory.Exists(path)) return [];

        var results = new List<T>();
        foreach (var file in Directory.EnumerateFiles(path, "*.json").OrderBy(x => x, StringComparer.Ordinal))
        {
            try
            {
                await using var stream = File.OpenRead(file);
                var item = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
                results.Add(item ?? throw new JsonException("Dosya boş."));
            }
            catch (JsonException exception)
            {
                throw new ContentValidationException($"{file} geçerli bir içerik dosyası değil: {exception.Message}");
            }
        }
        return results;
    }

    private static void Validate(
        IReadOnlyList<RubricDefinition> rubrics,
        IReadOnlyList<LearningContentDefinition> lessons,
        IReadOnlyList<LearningContentDefinition> patterns,
        IReadOnlyList<QuestionDefinition> questions)
    {
        EnsureUnique(rubrics.Select(x => (x.StableId, x.Version)), "rubric");
        EnsureUnique(lessons.Concat(patterns).Select(x => (x.StableId, x.Version)), "öğrenme içeriği");
        EnsureUnique(lessons.Concat(patterns).Select(x => (x.Slug, x.Version)), "içerik slug");
        EnsureUnique(questions.Select(x => (x.StableId, x.Version)), "soru");

        foreach (var rubric in rubrics)
        {
            Required(rubric.StableId, "Rubric stableId");
            Required(rubric.Title, $"Rubric {rubric.StableId} başlığı");
            Positive(rubric.Version, $"Rubric {rubric.StableId} sürümü");
            if (rubric.Dimensions.Count == 0 || rubric.Dimensions.Sum(x => x.Weight) != 100)
                throw new ContentValidationException($"Rubric {rubric.StableId} boyut ağırlıkları toplamı 100 olmalıdır.");
            EnsureUnique(rubric.Dimensions.Select(x => (x.Key, 1)), $"rubric {rubric.StableId} boyut anahtarı");
            EnsureUnique(rubric.Dimensions.Select(x => (x.Order.ToString(), 1)), $"rubric {rubric.StableId} boyut sırası");
        }

        foreach (var (content, kind) in lessons.Select(x => (x, "Ders")).Concat(patterns.Select(x => (x, "Pattern"))))
        {
            Required(content.StableId, $"{kind} stableId");
            Required(content.Slug, $"{kind} {content.StableId} slug");
            Required(content.Title, $"{kind} {content.StableId} başlığı");
            Positive(content.Version, $"{kind} {content.StableId} sürümü");
            Positive(content.EstimatedMinutes, $"{kind} {content.StableId} tahmini süresi");
            if (content.Sections.Count == 0)
                throw new ContentValidationException($"{kind} {content.StableId} en az bir bölüm içermelidir.");
            EnsureUnique(content.Sections.Select(x => (x.Key, 1)), $"{kind} {content.StableId} bölüm anahtarı");
            EnsureUnique(content.Sections.Select(x => (x.Order.ToString(), 1)), $"{kind} {content.StableId} bölüm sırası");
            if (kind == "Pattern") Required(content.Category, $"Pattern {content.StableId} kategorisi");
        }

        var rubricKeys = rubrics.Select(x => (x.StableId, x.Version)).ToHashSet();
        foreach (var question in questions)
        {
            Required(question.StableId, "Soru stableId");
            Required(question.Prompt, $"Soru {question.StableId} metni");
            Required(question.SkillSlug, $"Soru {question.StableId} skillSlug");
            Positive(question.Version, $"Soru {question.StableId} sürümü");
            if (!rubricKeys.Contains((question.RubricStableId, question.RubricVersion)))
                throw new ContentValidationException($"Soru {question.StableId}, dosyalarda bulunmayan rubric sürümüne bağlı.");
        }
    }

    private async Task UpsertRubricsAsync(IEnumerable<RubricDefinition> definitions, CancellationToken cancellationToken)
    {
        foreach (var definition in definitions)
        {
            var rubric = await db.Rubrics.Include(x => x.Dimensions)
                .SingleOrDefaultAsync(x => x.StableId == definition.StableId && x.Version == definition.Version, cancellationToken);
            rubric ??= new Rubric { Id = Guid.NewGuid(), StableId = definition.StableId, Version = definition.Version };
            if (db.Entry(rubric).State == EntityState.Detached) db.Rubrics.Add(rubric);
            rubric.Title = definition.Title;
            rubric.Description = definition.Description;
            rubric.Status = definition.Status;
            rubric.PublishedAt = PublishedAt(definition.Status, rubric.PublishedAt);
            var dimensionKeys = definition.Dimensions.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
            db.RubricDimensions.RemoveRange(rubric.Dimensions.Where(x => !dimensionKeys.Contains(x.Key)));
            foreach (var item in definition.Dimensions)
            {
                var dimension = rubric.Dimensions.SingleOrDefault(x => x.Key == item.Key);
                if (dimension is null)
                {
                    dimension = new RubricDimension { Id = Guid.NewGuid(), Rubric = rubric, Key = item.Key };
                    rubric.Dimensions.Add(dimension);
                }
                dimension.Label = item.Label;
                dimension.Description = item.Description;
                dimension.Weight = item.Weight;
                dimension.Order = item.Order;
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertContentAsync<T>(IEnumerable<LearningContentDefinition> definitions, CancellationToken cancellationToken)
        where T : VersionedContent, new()
    {
        var technologies = await db.Technologies.ToDictionaryAsync(x => x.Slug, cancellationToken);
        foreach (var definition in definitions)
        {
            var content = await db.Set<T>().Include(x => x.Sections)
                .SingleOrDefaultAsync(x => x.StableId == definition.StableId && x.Version == definition.Version, cancellationToken);
            content ??= new T { Id = Guid.NewGuid(), StableId = definition.StableId, Version = definition.Version };
            if (db.Entry(content).State == EntityState.Detached) db.Add(content);
            content.Slug = definition.Slug;
            content.Title = definition.Title;
            content.Summary = definition.Summary;
            content.Technology = Resolve(technologies, definition.TechnologySlug, $"İçerik {definition.StableId} teknolojisi");
            content.Level = definition.Level;
            content.EstimatedMinutes = definition.EstimatedMinutes;
            content.Status = definition.Status;
            content.ObjectivesJson = JsonSerializer.Serialize(definition.Objectives);
            content.PrerequisitesJson = JsonSerializer.Serialize(definition.Prerequisites);
            content.UpdatedAt = DateTimeOffset.UtcNow;
            content.PublishedAt = PublishedAt(definition.Status, content.PublishedAt);
            if (content is PatternGuide pattern) pattern.Category = definition.Category!;
            var sectionKeys = definition.Sections.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
            db.ContentSections.RemoveRange(content.Sections.Where(x => !sectionKeys.Contains(x.Key)));
            foreach (var item in definition.Sections)
            {
                var section = content.Sections.SingleOrDefault(x => x.Key == item.Key);
                if (section is null)
                {
                    section = new ContentSection { Id = Guid.NewGuid(), Content = content, Key = item.Key };
                    content.Sections.Add(section);
                }
                section.Title = item.Title;
                section.Order = item.Order;
                section.BodyMarkdown = item.BodyMarkdown;
                section.CodeLanguage = item.CodeLanguage;
                section.CodeSample = item.CodeSample;
            }
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertQuestionsAsync(IEnumerable<QuestionDefinition> definitions, CancellationToken cancellationToken)
    {
        var skills = await db.Skills.ToDictionaryAsync(x => x.Slug, cancellationToken);
        var technologies = await db.Technologies.ToDictionaryAsync(x => x.Slug, cancellationToken);
        var rubrics = await db.Rubrics.Include(x => x.Dimensions)
            .ToDictionaryAsync(x => (x.StableId, x.Version), cancellationToken);
        foreach (var definition in definitions)
        {
            var question = await db.Questions.SingleOrDefaultAsync(
                x => x.StableId == definition.StableId && x.Version == definition.Version, cancellationToken)
                ?? new Question { Id = Guid.NewGuid(), StableId = definition.StableId, Version = definition.Version };
            if (db.Entry(question).State == EntityState.Detached) db.Questions.Add(question);
            question.Prompt = definition.Prompt;
            question.Type = definition.Type;
            question.Level = definition.Level;
            question.Skill = Resolve(skills, definition.SkillSlug, $"Soru {definition.StableId} yetkinliği")!;
            question.Technology = Resolve(technologies, definition.TechnologySlug, $"Soru {definition.StableId} teknolojisi");
            question.Rubric = rubrics[(definition.RubricStableId, definition.RubricVersion)];
            question.ModelAnswer = definition.ModelAnswer;
            question.ExpectedSignalsJson = JsonSerializer.Serialize(definition.ExpectedSignals);
            question.RedFlagsJson = JsonSerializer.Serialize(definition.RedFlags);
            question.RubricJson = JsonSerializer.Serialize(question.Rubric.Dimensions.ToDictionary(x => x.Key, x => x.Weight));
            question.Status = definition.Status;
            question.PublishedAt = PublishedAt(definition.Status, question.PublishedAt);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static T? Resolve<T>(IReadOnlyDictionary<string, T> values, string? key, string label) where T : class
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        return values.TryGetValue(key, out var value)
            ? value
            : throw new ContentValidationException($"{label} bulunamadı: {key}");
    }

    private static DateTimeOffset? PublishedAt(PublicationStatus status, DateTimeOffset? current)
        => status == PublicationStatus.Published ? current ?? DateTimeOffset.UtcNow : null;

    private static void Required(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ContentValidationException($"{label} zorunludur.");
    }

    private static void Positive(int value, string label)
    {
        if (value <= 0) throw new ContentValidationException($"{label} sıfırdan büyük olmalıdır.");
    }

    private static void EnsureUnique(IEnumerable<(string Key, int Version)> values, string label)
    {
        var duplicate = values.GroupBy(x => x).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
            throw new ContentValidationException($"{label} benzersiz olmalıdır: {duplicate.Key.Key}@{duplicate.Key.Version}");
    }
}
