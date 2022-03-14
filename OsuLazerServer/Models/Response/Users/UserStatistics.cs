using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.Users;

public class UserStatistics
{
    [JsonPropertyName(@"level")] public LevelInfo Level { get; set; }

    public struct LevelInfo
    {
        [JsonPropertyName(@"current")] public int Current { get; set; }

        [JsonPropertyName(@"progress")] public int Progress { get; set; }
    }

    [JsonPropertyName(@"is_ranked")] public bool IsRanked { get; set; }

    [JsonPropertyName(@"global_rank")] public int? GlobalRank { get; set; }

    [JsonPropertyName(@"country_rank")] public int? CountryRank { get; set; }

    [JsonPropertyName(@"pp")] public decimal? PP { get; set; }

    [JsonPropertyName(@"ranked_score")] public long RankedScore { get; set; }

    [JsonPropertyName(@"hit_accuracy")] public double Accuracy { get; set; }

    [JsonPropertyName(@"play_count")] public int PlayCount { get; set; }

    [JsonPropertyName(@"play_time")] public int? PlayTime { get; set; }

    [JsonPropertyName(@"total_score")] public long TotalScore { get; set; }

    [JsonPropertyName(@"total_hits")] public long TotalHits { get; set; }

    [JsonPropertyName(@"maximum_combo")] public int MaxCombo { get; set; }

    [JsonPropertyName(@"replays_watched_by_others")]
    public int ReplaysWatched { get; set; }

    [JsonPropertyName(@"grade_counts")] public Grades GradesCount { get; set; }

    public struct Grades
    {
        [JsonPropertyName(@"ssh")] public int? SSPlus { get; set; }

        [JsonPropertyName(@"ss")] public int SS { get; set; }

        [JsonPropertyName(@"sh")] public int? SPlus { get; set; }

        [JsonPropertyName(@"s")] public int S { get; set; }

        [JsonPropertyName(@"a")] public int A { get; set; }
    }
}