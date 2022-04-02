using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Services.Rulesets;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
using HitResult = osu.Game.Rulesets.Scoring.HitResult;
using ServerStats = OsuLazerServer.Database.Tables.Scores.HitResultStats;

namespace OsuLazerServer.Services.Commands;

public class Commands
{
    public IServiceProvider Provider { get; set; }
    public Commands(IServiceProvider provider)
    {
        Provider = provider;
    }
    [Command("recalculate", "Recalculate user perfomance", -1, true)]
    public string GetHelp(CommandContext cmdCtx)
    {
        var context = new LazerContext();
        Task.Run(async () =>
        {
            Console.WriteLine("=====================");
            Console.WriteLine("Recalculation started.");

            Console.WriteLine("Reset Performance.");

            var storage = Provider.GetService<IUserStorage>();
            var ctx = new LazerContext();

            foreach (var user in ctx.OsuStats)
            {
                user.PerformancePoints = 0;
                user.TotalScore = 0;
                user.RankedScore = 0;
            }

            foreach (var user in ctx.TaikoStats)
            {
                user.PerformancePoints = 0;
                user.TotalScore = 0;
                user.RankedScore = 0;
            }

            foreach (var user in ctx.FruitsStats)
            {
                user.PerformancePoints = 0;
                user.TotalScore = 0;
                user.RankedScore = 0;
            }

            foreach (var user in ctx.TaikoStats)
            {
                user.PerformancePoints = 0;
                user.TotalScore = 0;
                user.RankedScore = 0;
            }

            Console.WriteLine("Recalculating scores...");
            var scores = ctx.Scores;
            foreach (var score in scores.Where(c => c.Passed))
            {

                try
                {
                    var currentContext = new LazerContext();
                    var user = currentContext.Users.FirstOrDefault(c => c.Id == score.UserId);


                    if (user is null)
                    {
                        Console.WriteLine($"{score.UserId} is null");
                        continue;
                    }

                    if (user.Banned)
                    {
                        Console.WriteLine($"{score.UserId} is banned");
                        continue;
                    }

                    await user.FetchUserStats();



                    var rulesetManager = cmdCtx.Services.ServiceProvider.GetService<IRulesetManager>();
                    
                    if (rulesetManager.GetRuleset(score.RuleSetId) is null)
                    {
                        Console.WriteLine($"{score.UserId} has no ruleset");
                        continue;
                    }

                    var ruleset = rulesetManager.GetRuleset(score.RuleSetId).RulesetInfo;

                    var stats = ServerStats.FromJson(score.Statistics);
                    var beatmap = new ProcessorWorkingBeatmap(await BeatmapUtils.GetBeatmapStream(score.BeatmapId),
                        score.BeatmapId);
                    double perfomance = 0;
                    var isRanked = await BeatmapUtils.GetBeatmapStatus(score.BeatmapId) == "ranked";
                    if (isRanked)
                    {
                        perfomance = ruleset.CreateInstance().CreatePerformanceCalculator()?.Calculate(new ScoreInfo
                        {
                            Accuracy = score.Accuracy,
                            Combo = score.MaxCombo,
                            MaxCombo = score.MaxCombo,
                            User = new APIUser {Username = "owo"},
                            Ruleset = ruleset,
                            Date = DateTimeOffset.UtcNow,
                            Passed = score.Passed,
                            TotalScore = score.TotalScore,
                            Statistics = new Dictionary<HitResult, int>()
                            {
                                [HitResult.Perfect] = stats.Perfect,
                                [HitResult.Good] = stats.Goods,
                                [HitResult.Great] = stats.Greats,
                                [HitResult.Meh] = stats.Meh,
                                [HitResult.Miss] = stats.Misses,
                                [HitResult.Ok] = stats.Ok,
                                [HitResult.None] = stats.None
                            },
                            HasReplay = false
                        }, beatmap).Total ?? 0;
                    }


                    score.PerfomancePoints = perfomance;


                    var userStats = user.GetStats(score.RuleSetId switch
                    {
                        0 => "osu",
                        1 => "taiko",
                        2 => "fruits",
                        3 => "mania",
                        _ => "osu"
                    });

                    var unrankedMods = new[] {"AT", "AA", "CN", "DA", "RX"};


                    if (score.Mods.Any(mod => unrankedMods.Contains(mod)))
                    {
                        score.Status = DbScoreStatus.OUTDATED;
                        score.PerfomancePoints = 0;
                        Console.WriteLine(
                            $"Score {score.Id} by {score.UserId} using unranked mods ({String.Join("", score.Mods)}), Unranking this score.");
                    }


                    if (score.Status == DbScoreStatus.BEST)
                    {
                        userStats.TotalScore = score.TotalScore;
                    }

                    if (isRanked && score.Status == DbScoreStatus.BEST)
                    {
                        userStats.PerformancePoints += (int) score.PerfomancePoints;
                        userStats.RankedScore += score.TotalScore;
                    }


                    Console.WriteLine(
                        $"{score.UserId} {beatmap.BeatmapInfo.GetDisplayTitle()} ({beatmap.BeatmapInfo.GetDisplayTitleRomanisable()}) => {perfomance}");

                    await currentContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Cannot recalculate score {score.Id}: {e.Message}");
                }

            }


            //saving scores.
            Console.WriteLine("=> Recalculating leaderboards");
            await storage.UpdateRankings("osu");
            await storage.UpdateRankings("taiko");
            await storage.UpdateRankings("fruits");
            await storage.UpdateRankings("mania");

            Console.WriteLine("Scores has recalculated.");
            await ctx.SaveChangesAsync();
        });
        return $"Recalculation has been started.";
    }

    [Command("reset", "Reset user's country.", 1, true)]
    public string ResetUserCountry(CommandContext ctx, string username)
    {
        var context = new LazerContext();

        var user = context.Users.AsEnumerable().FirstOrDefault(c => c.UsernameSafe == username.ToLower().Replace(' ', '_'));

        if (user is null)
            return "User not found";

        if (user.Country == "XX")
            return "This user don't have country already.";

        user.Country = "XX";

        context.SaveChanges();
        return "OK";
    }

    [Command("ban", "Ban a user", -1, true)]
    public string BanUser(CommandContext ctx, string username)
    {
        var context = new LazerContext();
        var storage = Provider.GetService<IUserStorage>();
        var distributedCache = Provider.GetService<IDistributedCache>();

        var user = context.Users.AsEnumerable().FirstOrDefault(c => c.UsernameSafe == username.ToLower().Replace(' ', '_'));

        if (user is null)
            return "User not found";
        
        user.Banned = !user.Banned;

        context.SaveChanges();
        var currentToken = storage.Users.FirstOrDefault(c => c.Value.Id == user.Id).Key;
        
        distributedCache.Remove($"token:{currentToken}");
        storage.UpdateRankings("osu");
        storage.UpdateRankings("taiko");
        storage.UpdateRankings("mania");
        storage.UpdateRankings("fruits");

        return $"{user.Username} has been {(user.Banned ? "banned" : "unbanned")}";
    }
    
    [Command("roll", "Roll a dice", 0, false, true)]
    public string RollDice(CommandContext ctx, string rawNumber = "100")
    { 
        var number = Convert.ToInt32(rawNumber);
        if (number <= 0)
            number = 100;
        
        var result = new Random().Next(1, number);

        return $"{ctx.User.Username} rolled {result}";
    }
}