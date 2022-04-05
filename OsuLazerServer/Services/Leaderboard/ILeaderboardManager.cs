using NUnit.Framework;
using osu.Game.Rulesets;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Rankings;
using OsuLazerServer.Models.Response.Scores;
using OsuLazerServer.Models.Score;

namespace OsuLazerServer.Services.Leaderboard;

public enum BeatmapLeaderboardType
{
    Global,
    Country,
    Friends
}

public interface ILeaderboardManager
{
    public Task<List<DbScore>?> GetBeatmapLeaderboard(int beatmapId, BeatmapLeaderboardType type, string? country,
        RulesetInfo? ruleset = null, int limit = 50);

    public Task<List<RankingUser>> GetLeaderboard(string mode, string? countryCode);
    public Task<UserScore?> GetUserScore(int beatmapId, int userId);

    public Task<APIScore?> ProcessScore(long token, APIScoreBody body, int beatmapId, int roomId,
        int playlistItem);
}