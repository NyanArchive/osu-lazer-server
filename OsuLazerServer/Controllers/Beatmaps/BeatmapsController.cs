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
using Beatmap = OsuLazerServer.Services.Beatmaps.Beatmap;
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
    [RequiredLazerClient]
    public async Task<IActionResult> LookUpBeatmapSetAsync([FromQuery(Name = "id")] int id, [FromQuery(Name = "checksum")] string? checksum)
    {
        var beatmap = await _resolver.FetchBeatmap(id);

        if (beatmap is null)
            return NotFound();

        return Json(beatmap.ToOsu());
    }
    
    [HttpGet("/api/v2/beatmapsets/lookup")]
    [RequiredLazerClient]
    public async Task<IActionResult> LookUpBeatmapAsync([FromQuery(Name = "beatmap_id")] int id, [FromQuery(Name = "checksum")] string? checksum)
    {
        var beatmap = await _resolver.FetchBeatmap(id);

        if (beatmap is null)
            return NotFound();

        var set = await _resolver.FetchSetAsync(beatmap.BeatmapsetId);

        if (set is null)
            return NotFound();
        return Json(set.ToBeatmapSet());
    }
    
    [HttpGet("scores")]
    [RequiredLazerClient]
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
    [RequiredLazerClient]
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
    [RequiredLazerClient]
    public async Task<IActionResult> SubmitScoreByToken([FromRoute(Name = "submitid")] int submitionToken, [FromRoute(Name = "id")] int beatmapId, [FromBody] APIScoreBody body)
    {

        if (!_storage.ScoreTokens.ContainsKey(submitionToken))
            return Unauthorized();

        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var ruleset = (RulesetId)body.RulesetId switch
        {
            RulesetId.Osu => new OsuRuleset().RulesetInfo,
            RulesetId.Taiko => new TaikoRuleset().RulesetInfo,
            RulesetId.Fruits => new CatchRuleset().RulesetInfo,
            RulesetId.Mania => new ManiaRuleset().RulesetInfo,
            _ => new OsuRuleset().RulesetInfo,
        };

        var mirrorBeatmap = await (await _resolver.FetchBeatmap(beatmapId)).ToOsu();
        var beatmapStream = await BeatmapUtils.GetBeatmapStream(beatmapId);
        var beatmap = new ProcessorWorkingBeatmap(beatmapStream, beatmapId);
        
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        var pp = ruleset.CreateInstance().CreatePerformanceCalculator()
            ?.Calculate(new ScoreInfo
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
            }, beatmap).Total;
        
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
            PerfomancePoints = pp??0,
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
        if (mirrorBeatmap.Status == LazerStatus.BeatmapOnlineStatus.Ranked && score.Passed)
        {
            stats.RankedScore += score.TotalScore;

            stats.PerfomancePoints += (int) Math.Floor(score.PerfomancePoints);
        }
        else
        {
            stats.PerfomancePoints = 0;
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
            await _storage.UpdatePerformance(ruleset.ShortName, user.Id, score.PerfomancePoints);
        }
        
        
        var unrankedMods = new []{"AT", "AA", "CN", "DA", "RX"};
        
        if (body.Mods.Any(mod => unrankedMods.Contains(mod.Acronym)))
        {
            score.Status = DbScoreStatus.OUTDATED;
        }
        
        await _context.SaveChangesAsync();

        _storage.LeaderboardCache.Remove(beatmapId);
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:perfomance");
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:score");
        ModeUtils.StatsCache.Remove($"{ruleset.ShortName}:{user.Id}");
        
        await _storage.UpdateRankings(ruleset.ShortName);
        

        return Json(new APIScore
        {
            Accuracy = score.Accuracy,
            Beatmap = await (await _resolver.FetchBeatmap(beatmapId))?.ToOsu()??null,
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


    [HttpGet("/api/v2/beatmaps")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetBeatmaps()
    {
        var uri = Request.QueryString.Value;
        
        var ids = uri?.Split("?").Last().Split("&").Select(s => s.Replace("ids[]=", "")).Select(val => Convert.ToInt32(val));
        return Json(new { beatmaps = await Task.WhenAll(ids?.Select(async d => await (await _resolver.FetchBeatmap(d)).ToOsu(_resolver)) ) });
    }

    [HttpPost("/api/v2/rooms/{roomId:int}/playlist/{playlistItem:int}/scores")]
    [RequiredLazerClient]
    public async Task<IActionResult> CreateMultiToken()
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var scoreToken = (DateTimeOffset.Now.ToUnixTimeMilliseconds() + new Random().Next(0, 30000)) / 1000;
        _storage.ScoreTokens.Add(scoreToken, user);

        return Json(new APIScoreToken
        {
            Id = scoreToken,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        });
    }
    
    [HttpPut("/api/v2/rooms/{roomId:int}/playlist/{playlistItem:int}/scores/{submitionToken}")]
    [RequiredLazerClient]
    public async Task<IActionResult> SubmitScore([FromRoute(Name = "roomId")] int roomId, [FromRoute(Name = "playlistItem")] int playlistItemId, [FromRoute(Name = "submitionToken")] int submitionToken, [FromBody] APIScoreBody body)
    {
        var room = _storage.Rooms[roomId];
        var hubRoom = _storage.HubRooms[roomId];
        var item = _storage.PlaylistItems[playlistItemId];
        

        if (!_storage.ScoreTokens.ContainsKey(submitionToken))
            return Unauthorized();

        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var ruleset = (RulesetId)body.RulesetId switch
        {
            RulesetId.Osu => new OsuRuleset().RulesetInfo,
            RulesetId.Taiko => new TaikoRuleset().RulesetInfo,
            RulesetId.Fruits => new CatchRuleset().RulesetInfo,
            RulesetId.Mania => new ManiaRuleset().RulesetInfo,
            _ => new OsuRuleset().RulesetInfo,
        };

        var mirrorBeatmap = await (await _resolver.FetchBeatmap(item.BeatmapId)).ToOsu();
        var beatmapStream = await BeatmapUtils.GetBeatmapStream(item.BeatmapId);
        var beatmap = new ProcessorWorkingBeatmap(beatmapStream, item.BeatmapId);
        
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

        var pp = ruleset.CreateInstance().CreatePerformanceCalculator()
            ?.Calculate(new ScoreInfo
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
            }, beatmap).Total;
        var score = new DbScore
        {
            Accuracy = body.Accuracy,
            Mods = body.Mods.Select(mod => mod.Acronym).ToList(),
            Rank = BeatmapUtils.ScoreRankFromString(body.Rank),
            Statistics = body.Statistics.ToJson(),
            UserId = user.Id,
            BeatmapId = item.BeatmapId,
            MaxCombo = body.MaxCombo,
            Passed = body.Passed,
            PerfomancePoints = pp??0,
            SubmittedIn = room.Id.Value,
            SubmittionPlaylist = item.ID,
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
        if (mirrorBeatmap.Status == LazerStatus.BeatmapOnlineStatus.Ranked)
        {
            stats.RankedScore += score.TotalScore;

            stats.PerfomancePoints += (int) Math.Floor(score.PerfomancePoints);
        }
        else
        {
            stats.PerfomancePoints = 0;
        }


        score.Status = DbScoreStatus.OUTDATED;
        var oldScore = await _context.Scores.FirstOrDefaultAsync(b => b.BeatmapId == item.BeatmapId && b.UserId == user.Id && b.Passed && b.Status == DbScoreStatus.BEST);
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
        
        
        var unrankedMods = new []{"AT", "AA", "CN", "DA"};
        
        if (body.Mods.Any(mod => unrankedMods.Contains(mod.Acronym)))
        {
            score.Status = DbScoreStatus.OUTDATED;
        }
        
        await _context.SaveChangesAsync();

        _storage.LeaderboardCache.Remove(item.BeatmapId);
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:perfomance");
        _storage.GlobalLeaderboardCache.Remove($"{ruleset.ShortName}:score");
        ModeUtils.StatsCache.Remove($"{ruleset.ShortName}:{user.Id}");
        return Json(new APIScore
        {
            Accuracy = score.Accuracy,
            Beatmap = await (await _resolver.FetchBeatmap(item.BeatmapId))?.ToOsu()??null,
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