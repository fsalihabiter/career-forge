using Microsoft.AspNetCore.Identity;

namespace CareerForge.Api.Models;

public enum ProficiencyLevel { Beginner = 1, Basic = 2, Intermediate = 3, Advanced = 4, Expert = 5 }
public enum PreparationSource { Skills, Specialization, JobRequirements, General }
public enum ContentMaturity { Preview, Beta, Complete }
public enum PublicationStatus { Draft, InReview, Published, Archived }
public enum SessionKind { Diagnostic, Interview }
public enum SessionStatus { Active, Completed }

public sealed class AppUser : IdentityUser<Guid>
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class UserProfile
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public PreparationSource Source { get; set; }
    public string TargetRole { get; set; } = "";
    public string TargetSeniority { get; set; } = "mid";
    public int ExperienceYears { get; set; }
    public string QuestionLanguage { get; set; } = "tr";
    public string PreferredCodeLanguage { get; set; } = "typescript";
    public int WeeklyStudyMinutes { get; set; } = 180;
    public DateOnly? InterviewDate { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class Technology
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public ContentMaturity Maturity { get; set; }
    public string Accent { get; set; } = "#3157d5";
}

public sealed class Skill
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class Specialization
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ICollection<SpecializationSkill> Skills { get; set; } = [];
}

public sealed class SpecializationSkill
{
    public Guid SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public bool Required { get; set; }
    public int Weight { get; set; }
}

public sealed class UserSkill
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SkillId { get; set; }
    public Guid? TechnologyId { get; set; }
    public ProficiencyLevel? SelfAssessedLevel { get; set; }
    public ProficiencyLevel? MeasuredLevel { get; set; }
    public ProficiencyLevel TargetLevel { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTimeOffset? LastAssessedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Skill Skill { get; set; } = null!;
    public Technology? Technology { get; set; }
}

public sealed class UserSpecialization
{
    public Guid UserId { get; set; }
    public Guid SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
}

public sealed class UserTechnology
{
    public Guid UserId { get; set; }
    public Guid TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
}

public abstract class VersionedContent
{
    public Guid Id { get; set; }
    public string StableId { get; set; } = "";
    public int Version { get; set; } = 1;
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public Guid? TechnologyId { get; set; }
    public Technology? Technology { get; set; }
    public ProficiencyLevel Level { get; set; } = ProficiencyLevel.Beginner;
    public int EstimatedMinutes { get; set; }
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public string ObjectivesJson { get; set; } = "[]";
    public string PrerequisitesJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
    public ICollection<ContentSection> Sections { get; set; } = [];
}

public sealed class Lesson : VersionedContent;

public sealed class PatternGuide : VersionedContent
{
    public string Category { get; set; } = "";
}

public sealed class ContentSection
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public VersionedContent Content { get; set; } = null!;
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public string BodyMarkdown { get; set; } = "";
    public string? CodeLanguage { get; set; }
    public string? CodeSample { get; set; }
}

public sealed class Rubric
{
    public Guid Id { get; set; }
    public string StableId { get; set; } = "";
    public int Version { get; set; } = 1;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
    public ICollection<RubricDimension> Dimensions { get; set; } = [];
    public ICollection<Question> Questions { get; set; } = [];
}

public sealed class RubricDimension
{
    public Guid Id { get; set; }
    public Guid RubricId { get; set; }
    public Rubric Rubric { get; set; } = null!;
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";
    public int Weight { get; set; }
    public int Order { get; set; }
}

public sealed class Question
{
    public Guid Id { get; set; }
    public string StableId { get; set; } = "";
    public int Version { get; set; } = 1;
    public string Prompt { get; set; } = "";
    public string Type { get; set; } = "";
    public ProficiencyLevel Level { get; set; }
    public string ModelAnswer { get; set; } = "";
    public string ExpectedSignalsJson { get; set; } = "[]";
    public string RedFlagsJson { get; set; } = "[]";
    public string RubricJson { get; set; } = "{}";
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid? RubricId { get; set; }
    public Guid SkillId { get; set; }
    public Guid? TechnologyId { get; set; }
    public Rubric? Rubric { get; set; }
    public Skill Skill { get; set; } = null!;
    public Technology? Technology { get; set; }
}

public sealed class InterviewSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SessionKind Kind { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public ICollection<SessionQuestion> Questions { get; set; } = [];
}

public sealed class SessionQuestion
{
    public Guid SessionId { get; set; }
    public InterviewSession Session { get; set; } = null!;
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int Order { get; set; }
    public string? AnswerText { get; set; }
    public int? SelfScore { get; set; }
    public string? EvaluationJson { get; set; }
    public DateTimeOffset? AnsweredAt { get; set; }
}

public sealed class LearningPath
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string SnapshotJson { get; set; } = "{}";
    public ICollection<LearningPathItem> Items { get; set; } = [];
}

public sealed class LearningPathItem
{
    public Guid Id { get; set; }
    public Guid LearningPathId { get; set; }
    public Guid SkillId { get; set; }
    public string Title { get; set; } = "";
    public string Reason { get; set; } = "";
    public int Order { get; set; }
    public bool Completed { get; set; }
}
