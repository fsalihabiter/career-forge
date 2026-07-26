using CareerForge.Api.Models;

namespace CareerForge.Api.Services;

public sealed record RubricEvaluation(
    string Rubric,
    int RubricVersion,
    double OverallScore,
    IReadOnlyList<RubricDimensionResult> Dimensions,
    IReadOnlyList<string> MatchedSignals,
    IReadOnlyList<string> MatchedRedFlags);

public sealed record RubricDimensionResult(
    string Key,
    string Label,
    int Weight,
    int Score,
    string Feedback);

public static class RubricEvaluator
{
    public static RubricEvaluation Evaluate(Question question, string answer)
    {
        var signals = ParseList(question.ExpectedSignalsJson);
        var redFlags = ParseList(question.RedFlagsJson);
        var matchedSignals = signals.Where(x => Contains(answer, x)).ToArray();
        var matchedRedFlags = redFlags.Where(x => Contains(answer, x)).ToArray();
        var signalCoverage = signals.Length == 0 ? 0.5 : (double)matchedSignals.Length / signals.Length;
        var wordCount = answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        var results = question.Rubric!.Dimensions.OrderBy(x => x.Order).Select(dimension =>
        {
            var score = dimension.Key switch
            {
                "technicalAccuracy" => Clamp(25 + (int)Math.Round(signalCoverage * 75) - matchedRedFlags.Length * 20),
                "analysis" => Clamp(20 + Math.Min(45, wordCount * 2) + MarkerScore(answer,
                    ["çünkü", "neden", "varsay", "kanıt", "ölç", "doğrula"], 7)),
                "tradeOff" => Clamp(15 + Math.Min(25, wordCount) + MarkerScore(answer,
                    ["trade-off", "alternatif", "ancak", "bedel", "risk", "yerine"], 12)),
                "communication" => Clamp(30 + Math.Min(50, wordCount * 2) +
                    (answer.Contains('.') || answer.Contains(';') ? 10 : 0)),
                _ => Clamp(25 + (int)Math.Round(signalCoverage * 75))
            };

            return new RubricDimensionResult(
                dimension.Key,
                dimension.Label,
                dimension.Weight,
                score,
                Feedback(dimension.Key, score, matchedSignals, matchedRedFlags));
        }).ToArray();

        var overall = Math.Round(results.Sum(x => x.Score * x.Weight) / 100d, 1);
        return new RubricEvaluation(
            question.Rubric.Title,
            question.Rubric.Version,
            overall,
            results,
            matchedSignals,
            matchedRedFlags);
    }

    private static string Feedback(
        string key,
        int score,
        IReadOnlyList<string> signals,
        IReadOnlyList<string> redFlags)
    {
        if (key == "technicalAccuracy")
        {
            if (redFlags.Count > 0)
                return $"Riskli yaklaşım algılandı: {string.Join(", ", redFlags)}.";
            if (signals.Count > 0)
                return $"Beklenen kanıtlardan eşleşenler: {string.Join(", ", signals)}.";
            return "Beklenen teknik sinyalleri daha açık adlandır.";
        }

        return score switch
        {
            >= 75 => "Bu boyut cevapta açık ve destekli biçimde görülüyor.",
            >= 50 => "Bu boyut mevcut; gerekçe ve somut örnekle güçlendirilebilir.",
            _ => key switch
            {
                "analysis" => "Varsayımları, nedeni ve doğrulama yöntemini açıkla.",
                "tradeOff" => "Bir alternatifin fayda, bedel ve riskini karşılaştır.",
                "communication" => "Cevabı kısa adımlar ve tamamlanmış cümlelerle yapılandır.",
                _ => "Bu boyut için daha somut kanıt sun."
            }
        };
    }

    private static int MarkerScore(string answer, string[] markers, int points) =>
        markers.Count(marker => Contains(answer, marker)) * points;

    private static bool Contains(string answer, string value) =>
        answer.Contains(value, StringComparison.CurrentCultureIgnoreCase);

    private static int Clamp(int score) => Math.Clamp(score, 0, 100);

    private static string[] ParseList(string json) =>
        System.Text.Json.JsonSerializer.Deserialize<string[]>(json) ?? [];
}
