using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Models.Rankings;

public class RankingUser : Statistics
{

    [JsonPropertyName("user")] public APIUser User { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }


    public static RankingUser FromUser(Statistics stats, APIUser user, IUserStorage storage, int mode = 0)
    {
        return new RankingUser
        {
            User = user,
            Level = stats.Level,
            Rank = stats.Rank,
            CountryRank = stats.CountryRank,
            GlobalRank =  storage.GetUserRank(user.Id, mode).GetAwaiter().GetResult(),
            GradeCounts = stats.GradeCounts,
            HitAccuracy = storage.GetUserHitAccuracy(user.Id, mode).GetAwaiter().GetResult(), //Rewrite this.
            IsRanked = true,
            IsActive = true,
            MaximumCombo = stats.MaximumCombo,
            PlayCount = stats.PlayCount,
            PlayTime = stats.PlayTime,
            PerfomancePoints = storage.GetUserPerfomancePoints(user.Id, mode).GetAwaiter().GetResult(),
            RankedScore = stats.RankedScore,
            TotalHits = stats.TotalHits,
            TotalScore = stats.TotalScore,
            ReplaysWatchedByOthers = stats.ReplaysWatchedByOthers
        };
    }
}