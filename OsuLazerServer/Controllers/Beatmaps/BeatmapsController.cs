using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Response.Scores;
using OsuLazerServer.Models.Score;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
using UniqueIdGenerator.Net;
using APIScore = OsuLazerServer.Models.Response.Scores.APIScore;
using HitResult = osu.Game.Rulesets.Scoring.HitResult;
using LazerStatus = OsuLazerServer.Models.Response.Beatmaps;

namespace OsuLazerServer.Controllers.Beatmaps;

[ApiController]
[Route("/api/v2/beatmaps/{id:int}")]
public class BeatmapsController : Controller
{
    private LazerContext _context;
    private IBeatmapSetResolver _resolver;
    private IUserStorage _storage;

    public BeatmapsController(LazerContext context, IBeatmapSetResolver resolver, IUserStorage storage)
    {
        _context = context;
        _resolver = resolver;
        _storage = storage;
    }

    [HttpGet("/api/v2/beatmaps/lookup")]
    [Authorization]
    public async Task<IActionResult> LookUpBeatmapAsync([FromQuery(Name = "id")] int id, [FromQuery(Name = "checksum")] string checksum)
    {

        var beatmap = await _resolver.FetchBeatmap(id);

        if (beatmap is null)
            return NotFound();

        return Json(beatmap.ToOsu());
    }
    
    [HttpGet("scores")]
    [Authorization]
    public async Task<IActionResult> GetScoresAsync([FromRoute(Name = "id")] int beatmapId, [FromQuery(Name = "type")] string type, [FromQuery(Name ="mode")] string mode)
    {
        
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        
        var beatmap = await _resolver.FetchBeatmap(beatmapId);

        var rulesetId = (RulesetId) Enum.Parse(typeof(RulesetId), string.Concat(mode[0].ToString().ToUpper(), mode.AsSpan(1)));
        
        var scores = _storage.LeaderboardCache.ContainsKey(beatmap.Id) ? _storage.LeaderboardCache[beatmap.Id] : _context.Scores.AsEnumerable().Where(score => score.BeatmapId == beatmapId && score.Passed && score.RuleSetId == (int) rulesetId && score.Status == DbScoreStatus.BEST);

        _storage.LeaderboardCache.TryAdd(beatmap.Id, scores.ToList());
        DbScore[] dbScores;

        if (type == "country")
        {
            dbScores = scores.Where(c => c.GetUser(_context).Country == user.Country).ToArray();
        }
        else
        {
            dbScores = scores.ToArray();
        }
        
    

        var userScore = dbScores.FirstOrDefault(s => s.UserId == user.Id);
        return Json(new ScoresResponse
        {
            Scores = _storage.LeaderboardCache[beatmap.Id].OrderByDescending(d => d.TotalScore).Select(c => c.ToOsuScore(_resolver).GetAwaiter().GetResult()).Take(50).ToList(),
            UserScore = new UserScore
            {
                Position = dbScores.Select((s, i) => new {Item = s, Position = i}).FirstOrDefault(s => s.Item.UserId == user.Id)?.Position??null,
                Score = userScore is not null ? await userScore.ToOsuScore() : null
            }
        });


    }
    [HttpPost("solo/scores")]
    [Authorization]
    public async Task<IActionResult> CreateSubmissionToken([FromRoute(Name = "id")] int beatmapId)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var scoreToken = DateTimeOffset.Now.ToUnixTimeSeconds();
        _storage.ScoreTokens.Add(scoreToken, user);

        return Json(new APIScoreToken
        {
            Id = scoreToken,
            Beatmap = beatmapId,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        });
    }

    [HttpPut("solo/scores/{submitId:int}")]
    [Authorization]
    public async Task<IActionResult> SubmitScoreByToken([FromRoute(Name = "submitid")] int submitionToken, [FromRoute(Name = "id")] int beatmapId, [FromBody] APIScoreBody body)
    {

        if (!_storage.ScoreTokens.ContainsKey(submitionToken))
            return Unauthorized();

        var unrankedMods = new []{"AT"};
        if (body.Mods.Any(mod => unrankedMods.Contains(mod.Acronym)))
        {
            return BadRequest();
        }
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var ruleset = (RulesetId)body.RulesetId switch
        {
            RulesetId.Osu => new OsuRuleset().RulesetInfo,
            RulesetId.Taiko => new TaikoRuleset().RulesetInfo,
            RulesetId.Fruits => new CatchRuleset().RulesetInfo,
            RulesetId.Mania => new ManiaRuleset().RulesetInfo,
            _ => new OsuRuleset().RulesetInfo,
        };

        var mirrorBeatmap = await _resolver.FetchBeatmap(beatmapId);
        var beatmapStream = await BeatmapUtils.GetBeatmapStream(beatmapId);
        var beatmap = new ProcessorWorkingBeatmap(beatmapStream, beatmapId);
        
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        
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
            PerfomancePoints = ruleset.CreateInstance().CreatePerformanceCalculator(beatmap, new ScoreInfo
            {
                Accuracy = body.Accuracy,
                Combo = body.MaxCombo,
                MaxCombo = body.MaxCombo,
                User = new APIUser { Username = "owo" },
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
            })
                ?.Calculate().Total,
            SubmittedAt = DateTimeOffset.UtcNow,
            TotalScore = body.TotalScore,
            RuleSetId = body.RulesetId
        };

        var entry = await _context.Scores.AddAsync(score);


        dbUser.PlayCount++;
        var stats = ModeUtils.FetchUserStats(_context, ruleset.ShortName, user.Id);
        

        if (score.MaxCombo > stats.MaxCombo)
        {
            stats.MaxCombo = score.MaxCombo;
        }

        stats.TotalScore += score.TotalScore;
        if (mirrorBeatmap.ToOsu().Status == LazerStatus.BeatmapOnlineStatus.Ranked)
        {
            stats.RankedScore += score.TotalScore;
            
            stats.PerfomancePoints += (int) Math.Floor(score.PerfomancePoints??0);
        }   


        score.Status = DbScoreStatus.OUTDATED;
        var oldScore = await _context.Scores.FirstOrDefaultAsync(b => b.BeatmapId == beatmapId && b.UserId == user.Id && b.Passed && b.Status == DbScoreStatus.BEST);
        if (score.TotalScore > (oldScore?.TotalScore ?? 0) && score.Passed)
        {
            score.Status = DbScoreStatus.BEST;
            if (oldScore is not null)
            {
                oldScore.Status = DbScoreStatus.OUTDATED;
            }

            if (_context.Scores.AsEnumerable().Any(c => c.UserId == user.Id && c.Passed))
            {
                stats.Accuracy = (float)(_context.Scores.Where(s => s.Passed && s.UserId == user.Id).Select(a => a.Accuracy * 100)
                    .ToList().Average() / 100F);
            }
        }
        
        await _context.SaveChangesAsync();

        _storage.LeaderboardCache.Remove(beatmapId);
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:perfomance");
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:score");
        return Json(new APIScore
        {
            Accuracy = score.Accuracy,
            Beatmap = (await _resolver.FetchBeatmap(beatmapId))?.ToOsu()??null,
            beatmapSet = (await _resolver.FetchSetAsync(beatmap.BeatmapInfo.BeatmapSet.OnlineID))?.ToBeatmapSet(),
            Date = score.SubmittedAt,
            Rank = Enum.GetName(score.Rank),
            Statistics = JsonSerializer.Deserialize<object>(score.Statistics) ?? new {},
            HasReplay = false,
            User = user.ToOsuUser(ruleset.ShortName),
            MaxCombo = score.MaxCombo,
            Mods = score.Mods.Select(s => new APIMod
            {
                Acronym = s,
                Settings = new Dictionary<string, object> {}
            }).ToArray(),
            PP = score.PerfomancePoints,
            TotalScore = score.TotalScore,
            OnlineID = entry.Entity.Id,
            RulesetID = score.RuleSetId
        });
    }
}