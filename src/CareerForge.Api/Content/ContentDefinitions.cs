using CareerForge.Api.Models;

namespace CareerForge.Api.Content;

public sealed record RubricDefinition(
    string StableId,
    int Version,
    string Title,
    string Description,
    PublicationStatus Status,
    IReadOnlyList<RubricDimensionDefinition> Dimensions);

public sealed record RubricDimensionDefinition(
    string Key,
    string Label,
    string Description,
    int Weight,
    int Order);

public sealed record LearningContentDefinition(
    string StableId,
    int Version,
    string Slug,
    string Title,
    string Summary,
    string? TechnologySlug,
    ProficiencyLevel Level,
    int EstimatedMinutes,
    PublicationStatus Status,
    IReadOnlyList<string> Objectives,
    IReadOnlyList<string> Prerequisites,
    string? Category,
    IReadOnlyList<ContentSectionDefinition> Sections);

public sealed record ContentSectionDefinition(
    string Key,
    string Title,
    int Order,
    string BodyMarkdown,
    string? CodeLanguage,
    string? CodeSample);

public sealed record QuestionDefinition(
    string StableId,
    int Version,
    string Prompt,
    string Type,
    ProficiencyLevel Level,
    string SkillSlug,
    string? TechnologySlug,
    string RubricStableId,
    int RubricVersion,
    string ModelAnswer,
    IReadOnlyList<string> ExpectedSignals,
    IReadOnlyList<string> RedFlags,
    PublicationStatus Status);

public sealed record ContentImportReport(int Rubrics, int Lessons, int Patterns, int Questions);

public sealed class ContentValidationException(string message) : Exception(message);
