using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Models.Rankings;

public class RankingUser : Statistics
{

    [JsonPropertyName("user")] public APIUser User { get; set; }
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }


    public static async Task<RankingUser> FromUser(APIUser user, IUserStorage storage, int mode = 0)
    {
        return new RankingUser
        {
            User = user,
            Level = user.Statistics.Level,
            Rank = user.Statistics.Rank,
            CountryRank = user.Statistics.CountryRank,
            GlobalRank =  await storage.GetUserRank(user.Id, mode),
            GradeCounts = user.Statistics.GradeCounts,
            HitAccuracy = await storage.GetUserHitAccuracy(user.Id, mode), //Rewrite this.
            IsRanked = (await storage.GetUserPerformancePoints(user.Id, mode) > 1),
            IsActive = true,
            MaximumCombo = user.Statistics.MaximumCombo,
            PlayCount = user.Statistics.PlayCount,
            PlayTime = user.Statistics.PlayTime,
            PerfomancePoints = await storage.GetUserPerformancePoints(user.Id, mode),
            RankedScore = user.Statistics.RankedScore,
            TotalHits = user.Statistics.TotalHits,
            TotalScore = user.Statistics.TotalScore,
            ReplaysWatchedByOthers = user.Statistics.ReplaysWatchedByOthers
        };
    }
}