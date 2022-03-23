using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables.Scores;
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
    public string GetHelp()
    {
        var context = new LazerContext();
        Task.Run(async () =>
        {
            Console.WriteLine("=====================");
            Console.WriteLine("Recalculation started.");
            
            Console.WriteLine("Reset Perfomance.");

            var ctx = new LazerContext();
            
            foreach (var user in ctx.OsuStats)
            {
                user.PerfomancePoints = 0;
            }
            foreach (var user in ctx.TaikoStats)
            {
                user.PerfomancePoints = 0;
            }
            foreach (var user in ctx.FruitsStats)
            {
                user.PerfomancePoints = 0;
            }
            foreach (var user in ctx.TaikoStats)
            {
                user.PerfomancePoints = 0;
            }
            
            Console.WriteLine("Recalculating scores...");
            var scores = ctx.Scores;
            foreach (var score in scores.Where(c => c.Status == DbScoreStatus.BEST && c.Passed))
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

                    if (score.Mods.Contains("RX"))
                    {
                        continue;
                    }

                    var ruleset = (RulesetId) score.RuleSetId switch
                    {
                        RulesetId.Osu => new OsuRuleset().RulesetInfo,
                        RulesetId.Taiko => new TaikoRuleset().RulesetInfo,
                        RulesetId.Fruits => new CatchRuleset().RulesetInfo,
                        RulesetId.Mania => new ManiaRuleset().RulesetInfo,
                        _ => new OsuRuleset().RulesetInfo,
                    };

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
                        }, beatmap).Total ?? 0;}


                    score.PerfomancePoints = perfomance;
                    var userStats = user.GetStats(score.RuleSetId switch
                    {
                        0 => "osu",
                        1 => "taiko",
                        2 => "fruits",
                        3 => "mania",
                        _ => "osu"
                    });
                    userStats.PerfomancePoints += (int) score.PerfomancePoints;
                    userStats.TotalScore += score.TotalScore;

                    if (isRanked)
                        userStats.RankedScore += score.TotalScore;
           

                    Console.WriteLine(
                        $"{score.UserId} {beatmap.BeatmapInfo.GetDisplayTitle()} ({beatmap.BeatmapInfo.GetDisplayTitleRomanisable()}) => {perfomance}");

                    await currentContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Cannot recalculate score {score.Id}: {e}");
                }
               
            }
            
            //saving scores.

            Console.WriteLine("Scores has recalculated.");
            await ctx.SaveChangesAsync();
        });
        return $"Recalculation has been started.";
    }
}