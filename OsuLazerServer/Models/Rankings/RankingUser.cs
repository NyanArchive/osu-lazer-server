using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;

namespace OsuLazerServer.Models.Rankings;

public class RankingUser : Statistics
{

    [JsonPropertyName("user")] public APIUser User { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }


    public static RankingUser FromUser(Statistics stats, APIUser user)
    {
        return new RankingUser
        {
            User = user,
            Level = stats.Level,
            Rank = stats.Rank,
            CountryRank = stats.CountryRank,
            GlobalRank = stats.GlobalRank,
            GradeCounts = stats.GradeCounts,
            HitAccuracy = stats.HitAccuracy * 100,
            IsRanked = true,
            IsActive = true,
            MaximumCombo = stats.MaximumCombo,
            PlayCount = stats.PlayCount,
            PlayTime = stats.PlayTime,
            PP = stats.PP,
            RankedScore = stats.RankedScore,
            TotalHits = stats.TotalHits,
            TotalScore = stats.TotalScore,
            ReplaysWatchedByOthers = stats.ReplaysWatchedByOthers
        };
    }
}