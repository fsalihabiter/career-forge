using CareerForge.Api.Models;

namespace CareerForge.Api.Contracts;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, string DisplayName, bool OnboardingCompleted);

public sealed record PreparationProfileRequest(
    PreparationSource Source,
    string TargetRole,
    string TargetSeniority,
    int ExperienceYears,
    string QuestionLanguage,
    string PreferredCodeLanguage,
    int WeeklyStudyMinutes,
    DateOnly? InterviewDate,
    Guid[] SpecializationIds,
    Guid[] TechnologyIds,
    UserSkillRequest[] Skills);

public sealed record UserSkillRequest(
    Guid SkillId,
    Guid? TechnologyId,
    ProficiencyLevel? SelfAssessedLevel,
    ProficiencyLevel TargetLevel);

public sealed record StartSessionRequest(int QuestionCount = 8);
public sealed record AnswerRequest(string AnswerText, int SelfScore);

public sealed record CatalogTechnology(Guid Id, string Slug, string Name, string Category, string Maturity, string Accent);
public sealed record CatalogSkill(Guid Id, string Slug, string Name, string Category, string Description);
public sealed record CatalogSpecialization(Guid Id, string Slug, string Name, string Description, object[] Skills);

public sealed record LearningTechnology(
    Guid Id,
    string Slug,
    string Name,
    string Category,
    string Accent,
    int LessonCount);

public sealed record LessonSummary(
    string StableId,
    int Version,
    string Slug,
    string Title,
    string Summary,
    string Level,
    int EstimatedMinutes,
    CatalogTechnology? Technology);

public sealed record LessonSection(
    string Key,
    string Title,
    int Order,
    string BodyMarkdown,
    string? CodeLanguage,
    string? CodeSample);

public sealed record LessonDetail(
    string StableId,
    int Version,
    string Slug,
    string Title,
    string Summary,
    string Level,
    int EstimatedMinutes,
    CatalogTechnology? Technology,
    string[] Objectives,
    string[] Prerequisites,
    LessonSection[] Sections);

public sealed record UpdateLessonProgressRequest(
    string LastSectionKey,
    string[] CompletedSectionKeys);

public sealed record LessonProgressResponse(
    string LessonStableId,
    int LessonVersion,
    string LastSectionKey,
    string[] CompletedSectionKeys,
    int CompletedSections,
    int TotalSections,
    bool Completed,
    DateTimeOffset UpdatedAt);

public sealed record SkillProgressPoint(
    Guid SessionId,
    decimal SessionScore,
    decimal RollingScore,
    string MeasuredLevel,
    decimal ConfidenceScore,
    int EvidenceCount,
    int TotalEvidenceCount,
    DateTimeOffset AssessedAt);

public sealed record SkillProgressHistoryResponse(
    Guid UserSkillId,
    Guid SkillId,
    string Skill,
    string? Technology,
    SkillProgressPoint[] History);

public sealed record ReviewItemResponse(
    Guid Id,
    Guid QuestionId,
    string Prompt,
    string Type,
    string Level,
    Guid SkillId,
    string SkillSlug,
    string Skill,
    string? Technology,
    DateTimeOffset AddedAt,
    DateTimeOffset NextReviewAt,
    DateTimeOffset? LastReviewedAt,
    int IntervalDays,
    int RepetitionCount);

public sealed record CompleteReviewRequest(string Rating);

public sealed record PatternSummary(
    string StableId,
    int Version,
    string Slug,
    string Title,
    string Summary,
    string Category,
    string Level,
    int EstimatedMinutes,
    CatalogTechnology? Technology);

public sealed record PatternDetail(
    string StableId,
    int Version,
    string Slug,
    string Title,
    string Summary,
    string Category,
    string Level,
    int EstimatedMinutes,
    CatalogTechnology? Technology,
    string[] Objectives,
    string[] Prerequisites,
    LessonSection[] Sections);
