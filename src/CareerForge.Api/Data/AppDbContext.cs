using CareerForge.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<SpecializationSkill> SpecializationSkills => Set<SpecializationSkill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserTechnology> UserTechnologies => Set<UserTechnology>();
    public DbSet<UserSpecialization> UserSpecializations => Set<UserSpecialization>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<PatternGuide> PatternGuides => Set<PatternGuide>();
    public DbSet<ContentSection> ContentSections => Set<ContentSection>();
    public DbSet<Rubric> Rubrics => Set<Rubric>();
    public DbSet<RubricDimension> RubricDimensions => Set<RubricDimension>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<SessionQuestion> SessionQuestions => Set<SessionQuestion>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("careerforge");
        modelBuilder.Entity<UserProfile>().HasKey(x => x.UserId);
        modelBuilder.Entity<UserProfile>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Technology>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Skill>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Specialization>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<SpecializationSkill>().HasKey(x => new { x.SpecializationId, x.SkillId });
        modelBuilder.Entity<UserSkill>().HasKey(x => x.Id);
        modelBuilder.Entity<UserSkill>().HasIndex(x => new { x.UserId, x.SkillId, x.TechnologyId }).IsUnique();
        modelBuilder.Entity<UserTechnology>().HasKey(x => new { x.UserId, x.TechnologyId });
        modelBuilder.Entity<UserSpecialization>().HasKey(x => new { x.UserId, x.SpecializationId });
        modelBuilder.Entity<SessionQuestion>().HasKey(x => new { x.SessionId, x.QuestionId });
        modelBuilder.Entity<VersionedContent>()
            .HasDiscriminator<string>("ContentType")
            .HasValue<Lesson>("lesson")
            .HasValue<PatternGuide>("pattern");
        modelBuilder.Entity<VersionedContent>().HasIndex(x => new { x.StableId, x.Version }).IsUnique();
        modelBuilder.Entity<VersionedContent>().HasIndex(x => new { x.Slug, x.Version }).IsUnique();
        modelBuilder.Entity<VersionedContent>().Property(x => x.ObjectivesJson).HasColumnType("jsonb");
        modelBuilder.Entity<VersionedContent>().Property(x => x.PrerequisitesJson).HasColumnType("jsonb");
        modelBuilder.Entity<ContentSection>().HasIndex(x => new { x.ContentId, x.Key }).IsUnique();
        modelBuilder.Entity<ContentSection>().HasIndex(x => new { x.ContentId, x.Order }).IsUnique();
        modelBuilder.Entity<Rubric>().HasIndex(x => new { x.StableId, x.Version }).IsUnique();
        modelBuilder.Entity<RubricDimension>().HasIndex(x => new { x.RubricId, x.Key }).IsUnique();
        modelBuilder.Entity<RubricDimension>().HasIndex(x => new { x.RubricId, x.Order }).IsUnique();
        modelBuilder.Entity<Question>().HasIndex(x => new { x.StableId, x.Version }).IsUnique();
        modelBuilder.Entity<Question>()
            .HasOne(x => x.Rubric)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.RubricId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Question>().Property(x => x.ExpectedSignalsJson).HasColumnType("jsonb");
        modelBuilder.Entity<Question>().Property(x => x.RedFlagsJson).HasColumnType("jsonb");
        modelBuilder.Entity<Question>().Property(x => x.RubricJson).HasColumnType("jsonb");
        modelBuilder.Entity<LearningPath>().Property(x => x.SnapshotJson).HasColumnType("jsonb");
        modelBuilder.Entity<LessonProgress>()
            .HasIndex(x => new { x.UserId, x.LessonStableId, x.LessonVersion })
            .IsUnique();
        modelBuilder.Entity<LessonProgress>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LessonProgress>().Property(x => x.CompletedSectionKeysJson).HasColumnType("jsonb");
        modelBuilder.Entity<UserSkill>().Property(x => x.ConfidenceScore).HasPrecision(5, 2);
    }
}
