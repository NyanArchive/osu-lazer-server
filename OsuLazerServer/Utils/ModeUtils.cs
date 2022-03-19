using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Utils;

public enum RulesetId
{
    [EnumMember(Value = "osu")] Osu = 0,
    [EnumMember(Value = "taiko")] Taiko = 1,
    [EnumMember(Value = "fruits")] Fruits = 2,
    [EnumMember(Value = "mania")] Mania = 3
}

public class ModeUtils
{
    public static Dictionary<string, IUserStats> StatsCache { get; set; } = new();
    public static Dictionary<string, int> CachedRanks { get; set; } = new();

    public static IUserStats GetStatsByMode(string mode, User user)
    {
        if (mode == "osu")
            return user.StatsOsu;
        if (mode == "taiko")
            return user.StatsTaiko;
        if (mode == "fruits")
            return user.StatsFruits;
        return mode == "mania" ? user.StatsMania : user.StatsOsu;
    }

    public static IUserStats FetchUserStats(LazerContext context, string mode, int id)
    {
        if (StatsCache.TryGetValue($"{id}:{mode}", out var value))
            return value;

        if (mode == "osu")
        {
            var userStats = context.OsuStats.FirstOrDefault(s => s.Id == id);
            StatsCache.TryAdd($"{id}:{mode}", userStats);
        }

        if (mode == "taiko")
        {
            var userStats = context.TaikoStats.FirstOrDefault(s => s.Id == id);
            StatsCache.TryAdd($"{id}:{mode}", userStats);
        }
        if (mode == "fruits")
        {
            var userStats = context.FruitsStats.FirstOrDefault(s => s.Id == id);
            StatsCache.TryAdd($"{id}:{mode}", userStats);
        }
        if (mode == "mania")
        {
            var userStats = context.ManiaStats.FirstOrDefault(s => s.Id == id);
            StatsCache.TryAdd($"{id}:{mode}", userStats);
        }
        var statsOsu = context.OsuStats.FirstOrDefault(s => s.Id == id);
        StatsCache.TryAdd($"{id}:{mode}", statsOsu);
        return statsOsu;
    }


    public static string CalculateRank(DbScore score)
    {
        var isHidden = score.Mods.Contains("HD") || score.Mods.Contains("FL");

        if (score.Accuracy == 1)
            return "X" + (isHidden ? "H" : "");
        if (score.Accuracy > .95 && score.Accuracy < 1)
            return "S" + (isHidden ? "H" : "");
        if (score.Accuracy < .95 && score.Accuracy > .90)
            return "A";
        if (score.Accuracy < .90 && score.Accuracy > .85)
            return "B";
        if (score.Accuracy < .85 && score.Accuracy > .80)
            return "C";
        if (score.Accuracy < .8)
            return "D";

        return "D";
    }

    public static int GetRank(string mode, int user)
    {
        if (CachedRanks.TryGetValue($"{mode}:{user}", out var rank))
            return rank;

        var context = new LazerContext();

        var leaderboard = context.Users.AsEnumerable()
            .OrderByDescending(d => (d.FetchStats(mode))?.PerfomancePoints ?? 0).Select((e, i) => new {index = i + 1, entry = e});

        var cachedRank = leaderboard.FirstOrDefault(u => u.entry.Id == user)?.index??0;

        CachedRanks.TryAdd($"{mode}:{user}", cachedRank);

        return cachedRank;
    }
}