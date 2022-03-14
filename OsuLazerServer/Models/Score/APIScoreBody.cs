using System.Text.Json.Serialization;
using OsuLazerServer.Database.Tables.Scores;

namespace OsuLazerServer.Models.Score;

public class APIScoreBody
{
    [JsonPropertyName("rank")] public string Rank { get; set; }

    [JsonPropertyName("total_score")] public int TotalScore { get; set; }

    [JsonPropertyName("accuracy")] public double Accuracy { get; set; }

    [JsonPropertyName("pp")] public double? PerfomancePoints { get; set; }

    [JsonPropertyName("max_combo")] public int MaxCombo { get; set; }

    [JsonPropertyName("ruleset_id")] public int RulesetId { get; set; }

    [JsonPropertyName("passed")] public bool Passed { get; set; }

    [JsonPropertyName("mods")] public List<Mod> Mods { get; set; }

    [JsonPropertyName("statistics")] public HitResultStats Statistics { get; set; }
}
public class Mod
{
    [JsonPropertyName("acronym")]
    public string Acronym { get; set; }

    [JsonPropertyName("settings")]
    public Dictionary<string, object> Settings { get; set; }
}