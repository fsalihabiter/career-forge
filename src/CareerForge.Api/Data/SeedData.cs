using CareerForge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerForge.Api.Data;

public static class SeedData
{
    public static async Task ApplyAsync(AppDbContext db)
    {
        if (await db.Technologies.AnyAsync()) return;

        var technologies = new[]
        {
            Tech("csharp", "C#", "Dil", ContentMaturity.Complete, "#7352c7"),
            Tech("dotnet", "ASP.NET Core", "Backend", ContentMaturity.Complete, "#6844b8"),
            Tech("java", "Java", "Dil", ContentMaturity.Beta, "#e76f36"),
            Tech("spring", "Spring Boot", "Backend", ContentMaturity.Beta, "#4a9f67"),
            Tech("typescript", "TypeScript", "Dil", ContentMaturity.Complete, "#2f65cf"),
            Tech("nodejs", "Node.js", "Backend", ContentMaturity.Beta, "#44883e"),
            Tech("react", "React", "Frontend", ContentMaturity.Complete, "#1689a8"),
            Tech("python", "Python", "Dil", ContentMaturity.Beta, "#d99b23"),
            Tech("postgresql", "PostgreSQL", "Veritabanı", ContentMaturity.Complete, "#336791"),
            Tech("docker", "Docker", "Platform", ContentMaturity.Beta, "#1976d2")
        };
        var skills = new[]
        {
            Skill("api-design", "API tasarımı", "Backend", "Güvenilir HTTP sözleşmeleri ve hata modeli"),
            Skill("debugging", "Debug yaklaşımı", "Mühendislik", "Kanıta dayalı kök neden analizi"),
            Skill("system-design", "Sistem tasarımı", "Mimari", "Sınır, ölçek ve tutarlılık kararları"),
            Skill("database", "Veritabanı", "Veri", "Modelleme, transaction ve sorgu performansı"),
            Skill("security", "Uygulama güvenliği", "Güvenlik", "Kimlik, yetki ve tehdit modelleme"),
            Skill("testing", "Test stratejisi", "Kalite", "Doğru test seviyesini ve kapsamını seçme"),
            Skill("frontend", "Frontend mimarisi", "Frontend", "State, erişilebilirlik ve performans"),
            Skill("observability", "Gözlemlenebilirlik", "Operasyon", "Log, metric ve trace ile teşhis"),
            Skill("delivery", "Teslimat ve Docker", "Platform", "Tekrarlanabilir build ve güvenli yayın")
        };
        var specializations = new[]
        {
            Spec("backend", "Backend Developer", "API, veri, güvenlik ve production dayanıklılığı"),
            Spec("frontend", "Frontend Developer", "Erişilebilir, hızlı ve sürdürülebilir web arayüzleri"),
            Spec("fullstack", "Full-Stack Developer", "Uçtan uca ürün geliştirme ve teknik karar"),
            Spec("database", "Database Developer", "Veri modelleme, sorgu ve operasyon"),
            Spec("devops", "DevOps / Cloud Engineer", "Teslimat, gözlemlenebilirlik ve platform"),
            Spec("architect", "Software Architect", "Sistem sınırları, ölçek ve trade-off"),
            Spec("qa", "QA / Test Automation", "Test mimarisi, otomasyon ve kalite sinyalleri")
        };
        db.AddRange(technologies);
        db.AddRange(skills);
        db.AddRange(specializations);
        await db.SaveChangesAsync();

        var skillBySlug = skills.ToDictionary(x => x.Slug);
        var specBySlug = specializations.ToDictionary(x => x.Slug);
        AddSpecSkills(db, specBySlug["backend"], skillBySlug, ["api-design", "database", "security", "testing", "debugging", "observability"]);
        AddSpecSkills(db, specBySlug["frontend"], skillBySlug, ["frontend", "testing", "debugging", "security"]);
        AddSpecSkills(db, specBySlug["fullstack"], skillBySlug, ["api-design", "frontend", "database", "testing", "debugging"]);
        AddSpecSkills(db, specBySlug["database"], skillBySlug, ["database", "debugging", "observability"]);
        AddSpecSkills(db, specBySlug["devops"], skillBySlug, ["delivery", "observability", "security", "debugging"]);
        AddSpecSkills(db, specBySlug["architect"], skillBySlug, ["system-design", "security", "observability", "database"]);
        AddSpecSkills(db, specBySlug["qa"], skillBySlug, ["testing", "debugging", "delivery"]);

        await db.SaveChangesAsync();
    }

    private static Technology Tech(string slug, string name, string category, ContentMaturity maturity, string accent)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Category = category, Maturity = maturity, Accent = accent };
    private static Skill Skill(string slug, string name, string category, string description)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Category = category, Description = description };
    private static Specialization Spec(string slug, string name, string description)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Description = description };
    private static void AddSpecSkills(AppDbContext db, Specialization spec, IReadOnlyDictionary<string, Skill> skills, string[] slugs)
    {
        for (var i = 0; i < slugs.Length; i++)
            db.Add(new SpecializationSkill { Specialization = spec, Skill = skills[slugs[i]], Required = i < 4, Weight = 100 - i * 8 });
    }
}
