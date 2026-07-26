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
