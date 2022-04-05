using System.Linq;
using System.Text.Json;
using BackgroundQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Rankings;
using OsuLazerServer.Models.Response.Scores;
using OsuLazerServer.Models.Score;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Replays;
using OsuLazerServer.Services.Rulesets;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
using APIScore = OsuLazerServer.Models.Response.Scores.APIScore;
using HitResult = osu.Game.Rulesets.Scoring.HitResult;
using LazerStatus = OsuLazerServer.Models.Response.Beatmaps;

namespace OsuLazerServer.Services.Leaderboard;

public class LeaderboardManager : ILeaderboardManager, IServiceScope
{
    #region Fields

    private IServiceScope _scope;
    public IServiceProvider ServiceProvider { get; }

    public static RulesetInfo DEFAULT_RULESET = new RulesetInfo {OnlineID = 0, Name = "osu!", ShortName = "osu!"};

    public LeaderboardManager(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _scope = ServiceProvider.CreateScope();
    }

    #endregion

    public async Task<List<DbScore>?> GetBeatmapLeaderboard(int beatmapId, BeatmapLeaderboardType type, string? country,
        RulesetInfo? ruleset = null, int limit = 50)
    {
        if (ruleset is null)
            ruleset = DEFAULT_RULESET;

        if (type == BeatmapLeaderboardType.Country && string.IsNullOrEmpty(country))
            return null;
        using var scope = ServiceProvider.CreateScope();

        var cache = scope.ServiceProvider.GetService<IMemoryCache>();

        if (cache.TryGetValue($"{beatmapId}_{type}_{country}_{ruleset.OnlineID}_{limit}", out var cached))
            return (List<DbScore>) cached;

        var db = scope.ServiceProvider.GetService<LazerContext>();


        var scores = db.Scores.ToList().GroupJoin(db.Users, x => x.Id, x => x.Id, (score, user) => new {score, user})
            .ToList().Where(c => c.user.FirstOrDefault()?.Banned ?? true && c.score.Status == DbScoreStatus.BEST &&
                c.score.BeatmapId == beatmapId && c.score.RuleSetId == ruleset.OnlineID).ToList();

        if (scores is null)
            return null;
        var leaderboard = new List<DbScore>();
        switch (type)
        {
            case BeatmapLeaderboardType.Global:
                leaderboard = scores.Where(c => c.score.Status == DbScoreStatus.BEST)
                    .OrderByDescending(x => x.score.TotalScore).Select(c => c.score).Take(limit)
                    .ToList();
                break;
            case BeatmapLeaderboardType.Country:
                leaderboard = scores.Where(c => c.score.Status == DbScoreStatus.BEST)
                    .OrderByDescending(c => c.score.TotalScore).Select(c => c.score)
                    .Take(limit)
                    .ToList();
                break;
        }

        //cache leaderboard
        cache.Set($"{beatmapId}_{type}_{country}_{ruleset.OnlineID}_{limit}", leaderboard, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
        });

        return leaderboard;
    }

    public Task<List<RankingUser>> GetLeaderboard(string mode, string? countryCode)
    {
        throw new NotImplementedException();
    }

    public async Task<UserScore?> GetUserScore(int beatmapId, int userId)
    {
        var beatmapLeaderboard = await GetBeatmapLeaderboard(beatmapId, BeatmapLeaderboardType.Global, null);

        if (beatmapLeaderboard is null)
            return null;

        var userScore = beatmapLeaderboard.Select((score, index) => new {Position = index + 1, score})
            .FirstOrDefault(x => x.score.UserId == userId);

        if (userScore is null)
            return null;

        return new UserScore
        {
            Position = userScore.Position,
            Score = await userScore.score.ToOsuScore()
        };
    }


    public async Task<APIScore?> ProcessScore(long token, APIScoreBody body, int beatmapId, int roomId, int playlistItem)
    {
        using var scope = ServiceProvider.CreateScope();
        
        var db = scope.ServiceProvider.GetService<LazerContext>();
        var storage = scope.ServiceProvider.GetService<IUserStorage>();
        var rulesetManager = scope.ServiceProvider.GetService<IRulesetManager>();
        var resolver = scope.ServiceProvider.GetService<IBeatmapSetResolver>();
        var taskQueue = scope.ServiceProvider.GetService<IBackgroundTaskQueue>();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        var replayManager = scope.ServiceProvider.GetService<IReplayManager>();

        if (!storage.ScoreTokens.ContainsKey(token))
            return null;

        var user = storage.ScoreTokens[token];


        var ruleset = rulesetManager.GetRuleset(body.RulesetId).RulesetInfo;

        var mirrorBeatmap = await (await resolver.FetchBeatmap(beatmapId)).ToOsu();
        var beatmapStream = await BeatmapUtils.GetBeatmapStream(beatmapId);
        var beatmap = new ProcessorWorkingBeatmap(beatmapStream, beatmapId);

        var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        double pp = 0;
        var scoreInfo = new ScoreInfo
        {
            Accuracy = body.Accuracy,
            Combo = body.MaxCombo,
            MaxCombo = body.MaxCombo,
            User = new APIUser {Username = "owo"},
            Ruleset = ruleset,
            Date = DateTimeOffset.UtcNow,
            Passed = body.Passed,
            TotalScore = body.TotalScore,
            Statistics = new Dictionary<HitResult, int>()
            {
                [HitResult.Perfect] = body.Statistics.Perfect,
                [HitResult.Good] = body.Statistics.Goods,
                [HitResult.Great] = body.Statistics.Greats,
                [HitResult.Meh] = body.Statistics.Meh,
                [HitResult.Miss] = body.Statistics.Misses,
                [HitResult.Ok] = body.Statistics.Ok,
                [HitResult.None] = body.Statistics.None
            },
            HasReplay = false
        };
        if (mirrorBeatmap.Status == LazerStatus.BeatmapOnlineStatus.Ranked)
        {
            pp = ruleset.CreateInstance().CreatePerformanceCalculator()
                ?.Calculate(scoreInfo, beatmap).Total ?? 0;
        }
        
        var score = new DbScore
        {
            Accuracy = body.Accuracy,
            Mods = body.Mods.Select(mod => mod.Acronym).ToList(),
            Rank = BeatmapUtils.ScoreRankFromString(body.Rank),
            Statistics = body.Statistics.ToJson(),
            UserId = user.Id,
            BeatmapId = beatmapId,
            MaxCombo = body.MaxCombo,
            Passed = body.Passed,
            PerfomancePoints = pp,
            SubmittedAt = DateTimeOffset.UtcNow,
            TotalScore = body.TotalScore,
            RuleSetId = body.RulesetId,
            SubmittionPlaylist = playlistItem,
            SubmittedIn = roomId
        };

        var entry = await db.Scores.AddAsync(score);


        dbUser.PlayCount++;

        IUserStats stats;

        if (ruleset.OnlineID > 3)
        {
            stats = await user.FetchRulesetStats(ruleset);
        }
        else
        {
            stats = ModeUtils.FetchUserStats(db, ruleset.ShortName, user.Id);
        }


        if (score.MaxCombo > stats.MaxCombo)
        {
            stats.MaxCombo = score.MaxCombo;
        }

        stats.TotalScore += score.TotalScore;
        if (mirrorBeatmap.Status == LazerStatus.BeatmapOnlineStatus.Ranked && score.Passed)
        {
            stats.RankedScore += score.TotalScore;

            await storage.UpdatePerformance(ruleset.ShortName, user.Id, score.PerfomancePoints);
        }
        else
        {
            stats.PerformancePoints = 0;
        }


        score.Status = DbScoreStatus.OUTDATED;
        var oldScore = await db.Scores.FirstOrDefaultAsync(b =>
            b.BeatmapId == beatmapId && b.UserId == user.Id && b.Passed && b.Status == DbScoreStatus.BEST);
        if (score.TotalScore > (oldScore?.TotalScore ?? 0) && score.Passed)
        {
            score.Status = DbScoreStatus.BEST;
            if (oldScore is not null)
            {
                oldScore.Status = DbScoreStatus.OUTDATED;
            }

            if (db.Scores.AsEnumerable().Any(c => c.UserId == user.Id && c.Passed))
            {
                stats.Accuracy = (float) (db.Scores.Where(s => s.Passed && s.UserId == user.Id)
                    .Select(a => a.Accuracy * 100)
                    .ToList().Average() / 100F);
            }
        }


        var unrankedMods = new[] {"AT", "AA", "CN", "DA", "RX"};

        if (body.Mods.Any(mod => unrankedMods.Contains(mod.Acronym)))
        {
            score.Status = DbScoreStatus.OUTDATED;
        }

        await db.SaveChangesAsync();
        taskQueue.Enqueue(async c =>
        {
            await storage.UpdateRankings(ruleset.ShortName);
                
            //update cache

            cache.Remove($"{beatmapId}_{BeatmapLeaderboardType.Global}__{ruleset.OnlineID}_{50}");
            cache.Remove($"{beatmapId}_{BeatmapLeaderboardType.Country}_{dbUser.Country}_{ruleset.OnlineID}_{50}");
        });
        storage.LeaderboardCache.Remove(beatmapId);
        storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:perfomance");
        storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:score");
        ModeUtils.StatsCache.Remove($"{ruleset.ShortName}:{user.Id}");

        var hasReplay = mirrorBeatmap.Status != LazerStatus.BeatmapOnlineStatus.Pending &&
                        mirrorBeatmap.Status != LazerStatus.BeatmapOnlineStatus.Graveyard &&
                        mirrorBeatmap.Status != LazerStatus.BeatmapOnlineStatus.WIP &&
                        mirrorBeatmap.Status != LazerStatus.BeatmapOnlineStatus.None;
        if (hasReplay && score.Passed)
        {
            using (var writter = File.OpenWrite(Path.Join("replays", $"{entry.Entity.Id}.osr")))
            {
                scoreInfo.OnlineID = entry.Entity.Id;
                var replayStream = await replayManager.GetReplayLegacyDataAsync(user.Id, scoreInfo);
                replayStream.Seek(0, SeekOrigin.Begin);
                await replayStream.CopyToAsync(writter);
            
                writter.Close();
                replayManager.ClearReplayFrames(user.Id);
            }
        }

        return new APIScore
        {
            Accuracy = score.Accuracy,
            Beatmap = await (await resolver.FetchBeatmap(beatmapId))?.ToOsu() ?? null,
            beatmapSet = (await resolver.FetchSetAsync(beatmap.BeatmapInfo.BeatmapSet.OnlineID))?.ToBeatmapSet(),
            Date = score.SubmittedAt,
            Rank = Enum.GetName(score.Rank),
            Statistics = JsonSerializer.Deserialize<object>(score.Statistics) ?? new { },
            HasReplay =  hasReplay && score.Passed,
            User = await user.ToOsuUser(ruleset.ShortName),
            MaxCombo = score.MaxCombo,
            Mods = score.Mods.Select(s => new APIMod
            {
                Acronym = s,
                Settings = new Dictionary<string, object> { }
            }).ToArray(),
            PP = score.PerfomancePoints,
            TotalScore = score.TotalScore,
            OnlineID = entry.Entity.Id,
            RulesetID = score.RuleSetId
        };
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}