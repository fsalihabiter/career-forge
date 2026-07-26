using System.Text.Json;
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

        var techBySlug = technologies.ToDictionary(x => x.Slug);
        var rubric = DefaultRubric();
        db.Add(rubric);
        var questions = new[]
        {
            Q("api-idempotency", "Kullanıcı aynı oluşturma isteğini iki kez gönderdiğinde çift kayıt oluşmasını nasıl önlersin?", "Tasarım", ProficiencyLevel.Intermediate, skillBySlug["api-design"], null, rubric,
                "İstemci işlem anahtarı, veritabanı unique constraint'i ve aynı transaction içinde sonuç kaydı kullanırım.",
                ["Idempotency key", "Unique constraint", "Transaction sınırı"], ["Sadece butonu kapatmak"]),
            Q("debug-no-log", "Bir API bazen 500 dönüyor ama loglarda hata yok. İlk 15 dakikada hangi kanıtları toplarsın?", "Incident", ProficiencyLevel.Advanced, skillBySlug["debugging"], null, rubric,
                "Correlation ID ile isteği izler, exception pipeline'ını, deployment farkını ve downstream sürelerini incelerim.",
                ["Correlation ID", "Downstream süreleri", "Deployment karşılaştırması"], ["Rastgele log açmak"]),
            Q("system-boundaries", "Yeni bir ürün için mikroservis yerine modüler monolit seçimini nasıl savunursun?", "Sistem tasarımı", ProficiencyLevel.Advanced, skillBySlug["system-design"], null, rubric,
                "Ekip, bağımsız ölçek ve deployment ihtiyacını ölçer; sınırları kod içinde koruyup dağıtık sistem maliyetini ertelerim.",
                ["Ekip sınırı", "Bağımsız ölçek", "Operasyon maliyeti"], ["Mikroservis her zaman iyidir"]),
            Q("postgres-index", "Filtreli bir sorgu büyüdükçe yavaşlıyor. PostgreSQL tarafında nasıl incelersin?", "Performans", ProficiencyLevel.Intermediate, skillBySlug["database"], techBySlug["postgresql"], rubric,
                "EXPLAIN ANALYZE, gerçek cardinality, buffer kullanımı ve sorgu desenine uygun bileşik/partial index incelerim.",
                ["EXPLAIN ANALYZE", "Cardinality", "Index sırası"], ["Her kolona index eklemek"]),
            Q("react-race", "Filtre hızla değiştiğinde eski HTTP cevabı yeni ekranı eziyor. React'te nasıl önlersin?", "Debug", ProficiencyLevel.Intermediate, skillBySlug["frontend"], techBySlug["react"], rubric,
                "AbortController veya query identity kullanır, response'un güncel filtreye ait olduğunu doğrularım.",
                ["AbortController", "Request identity", "Loading/error state"], ["Timeout eklemek"]),
            Q("dotnet-cancellation", "HTTP isteği iptal olduğunda veritabanı işinin de durmasını .NET'te nasıl sağlarsın?", "Kod okuma", ProficiencyLevel.Intermediate, skillBySlug["api-design"], techBySlug["dotnet"], rubric,
                "CancellationToken'ı endpoint'ten application ve EF Core async metoduna kadar taşırım.",
                ["Token propagation", "ToListAsync(token)", "İptali hata saymamak"], [".Result kullanmak"]),
            Q("spring-transactions", "Spring Boot servisinde transaction sınırını controller yerine servis katmanında neden kurarsın?", "Tasarım", ProficiencyLevel.Intermediate, skillBySlug["database"], techBySlug["spring"], rubric,
                "İş use-case'inin atomik sınırı servis katmanındadır; HTTP ve persistence ayrıntısını domain kararından ayırırım.",
                ["Use-case sınırı", "Atomiklik", "Rollback"], ["Her repository metoduna ayrı transaction"]),
            Q("node-event-loop", "Node.js API'de CPU yoğun bir işlem tüm istekleri yavaşlatıyor. Neyi ölçer ve nasıl ayırırsın?", "Performans", ProficiencyLevel.Advanced, skillBySlug["debugging"], techBySlug["nodejs"], rubric,
                "Event-loop lag ve CPU profilini ölçer; işi worker thread veya ayrı worker sürecine taşırım.",
                ["Event-loop lag", "CPU profile", "Worker"], ["Daha çok await eklemek"]),
            Q("python-async", "Python web servisinde async endpoint içinde bloklayan çağrı kullanmanın etkisi nedir?", "Kod okuma", ProficiencyLevel.Intermediate, skillBySlug["debugging"], techBySlug["python"], rubric,
                "Event loop bloklanır ve eşzamanlı istekler bekler; async istemci veya kontrollü thread offload kullanırım.",
                ["Event loop", "Blocking I/O", "Async client"], ["Her fonksiyonu async yapmak"]),
            Q("otel-triage", "Kullanıcı yavaşlık bildiriyor. Metric, trace ve log hangi sırayla yardımcı olur?", "Incident", ProficiencyLevel.Intermediate, skillBySlug["observability"], null, rubric,
                "Metric etkiyi ve zamanı, trace yavaş span'i, log ise bağlamsal nedeni bulmaya yardım eder.",
                ["Metric ile kapsam", "Trace ile yol", "Log ile neden"], ["Sadece log aramak"])
        };
        db.AddRange(questions);
        await db.SaveChangesAsync();
    }

    private static Technology Tech(string slug, string name, string category, ContentMaturity maturity, string accent)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Category = category, Maturity = maturity, Accent = accent };
    private static Skill Skill(string slug, string name, string category, string description)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Category = category, Description = description };
    private static Specialization Spec(string slug, string name, string description)
        => new() { Id = Guid.NewGuid(), Slug = slug, Name = name, Description = description };
    private static Question Q(string stableId, string prompt, string type, ProficiencyLevel level, Skill skill, Technology? tech, Rubric rubric, string answer, string[] signals, string[] redFlags)
        => new()
        {
            Id = Guid.NewGuid(), StableId = stableId, Prompt = prompt, Type = type, Level = level, Skill = skill,
            Technology = tech, Rubric = rubric, Status = PublicationStatus.Published, PublishedAt = DateTimeOffset.UtcNow,
            ModelAnswer = answer, ExpectedSignalsJson = JsonSerializer.Serialize(signals),
            RedFlagsJson = JsonSerializer.Serialize(redFlags),
            RubricJson = """{"technicalAccuracy":40,"analysis":25,"tradeOff":20,"communication":15}"""
        };
    private static Rubric DefaultRubric()
    {
        var rubric = new Rubric
        {
            Id = Guid.NewGuid(), StableId = "default-technical-answer", Title = "Teknik cevap değerlendirmesi",
            Description = "Teknik doğruluk, analiz, trade-off ve iletişim boyutları",
            Status = PublicationStatus.Published, PublishedAt = DateTimeOffset.UtcNow
        };
        rubric.Dimensions =
        [
            Dimension(rubric, "technicalAccuracy", "Teknik doğruluk", 40, 1),
            Dimension(rubric, "analysis", "Analiz", 25, 2),
            Dimension(rubric, "tradeOff", "Trade-off", 20, 3),
            Dimension(rubric, "communication", "İletişim", 15, 4)
        ];
        return rubric;
    }
    private static RubricDimension Dimension(Rubric rubric, string key, string label, int weight, int order)
        => new()
        {
            Id = Guid.NewGuid(), Rubric = rubric, Key = key, Label = label,
            Description = $"{label} değerlendirme boyutu", Weight = weight, Order = order
        };
    private static void AddSpecSkills(AppDbContext db, Specialization spec, IReadOnlyDictionary<string, Skill> skills, string[] slugs)
    {
        for (var i = 0; i < slugs.Length; i++)
            db.Add(new SpecializationSkill { Specialization = spec, Skill = skills[slugs[i]], Required = i < 4, Weight = 100 - i * 8 });
    }
}
