using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Models.Rankings;

public class RankingUser : Statistics
{

    [JsonPropertyName("user")] public APIUser User { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }


    public static async Task<RankingUser> FromUser(Statistics stats, APIUser user, IUserStorage storage, int mode = 0)
    {
        return new RankingUser
        {
            User = user,
            Level = stats.Level,
            Rank = stats.Rank,
            CountryRank = stats.CountryRank,
            GlobalRank =  await storage.GetUserRank(user.Id, mode),
            GradeCounts = stats.GradeCounts,
            HitAccuracy = await storage.GetUserHitAccuracy(user.Id, mode), //Rewrite this.
            IsRanked = true,
            IsActive = true,
            MaximumCombo = stats.MaximumCombo,
            PlayCount = stats.PlayCount,
            PlayTime = stats.PlayTime,
            PerfomancePoints = await storage.GetUserPerfomancePoints(user.Id, mode),
            RankedScore = stats.RankedScore,
            TotalHits = stats.TotalHits,
            TotalScore = stats.TotalScore,
            ReplaysWatchedByOthers = stats.ReplaysWatchedByOthers
        };
    }
}