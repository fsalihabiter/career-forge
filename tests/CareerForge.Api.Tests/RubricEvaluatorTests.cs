using CareerForge.Api.Models;
using CareerForge.Api.Services;

namespace CareerForge.Api.Tests;

public sealed class RubricEvaluatorTests
{
    [Fact]
    public void Evaluate_applies_weights_and_returns_explainable_dimension_feedback()
    {
        var question = QuestionWithRubric();

        var result = RubricEvaluator.Evaluate(
            question,
            "Idempotency key ve unique constraint kullanırım; çünkü transaction sınırı çift kaydı önler. Alternatif olarak kilit düşünülebilir ancak operasyonel bedeli vardır ve metriği ölçerek doğrularım.");

        Assert.Equal(4, result.Dimensions.Count);
        Assert.Equal(100, result.Dimensions.Sum(x => x.Weight));
        Assert.InRange(result.OverallScore, 0, 100);
        Assert.Contains("Idempotency key", result.MatchedSignals);
        Assert.All(result.Dimensions, dimension =>
        {
            Assert.InRange(dimension.Score, 0, 100);
            Assert.False(string.IsNullOrWhiteSpace(dimension.Feedback));
        });
    }

    [Fact]
    public void Evaluate_exposes_matched_red_flags_and_reduces_technical_score()
    {
        var question = QuestionWithRubric();

        var risky = RubricEvaluator.Evaluate(question, "Sadece butonu kapatmak yeterlidir.");
        var evidenced = RubricEvaluator.Evaluate(
            question,
            "Idempotency key, unique constraint ve transaction sınırı kullanırım.");

        Assert.Contains("Sadece butonu kapatmak", risky.MatchedRedFlags);
        Assert.True(
            risky.Dimensions.Single(x => x.Key == "technicalAccuracy").Score <
            evidenced.Dimensions.Single(x => x.Key == "technicalAccuracy").Score);
        Assert.Contains("Riskli yaklaşım", risky.Dimensions.Single(x => x.Key == "technicalAccuracy").Feedback);
    }

    private static Question QuestionWithRubric()
    {
        var rubric = new Rubric
        {
            Title = "Teknik cevap değerlendirmesi",
            Version = 1
        };
        rubric.Dimensions =
        [
            Dimension(rubric, "technicalAccuracy", "Teknik doğruluk", 40, 1),
            Dimension(rubric, "analysis", "Analiz", 25, 2),
            Dimension(rubric, "tradeOff", "Trade-off", 20, 3),
            Dimension(rubric, "communication", "İletişim", 15, 4)
        ];
        return new Question
        {
            Rubric = rubric,
            ExpectedSignalsJson = """["Idempotency key","Unique constraint","Transaction sınırı"]""",
            RedFlagsJson = """["Sadece butonu kapatmak"]"""
        };
    }

    private static RubricDimension Dimension(Rubric rubric, string key, string label, int weight, int order) =>
        new() { Rubric = rubric, Key = key, Label = label, Weight = weight, Order = order };
}
