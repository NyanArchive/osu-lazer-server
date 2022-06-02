using System.Text.Json;
using BackgroundQueue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Timing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Response.Scores;
using OsuLazerServer.Models.Score;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Leaderboard;
using OsuLazerServer.Services.Rulesets;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
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
    private IBackgroundTaskQueue _taskQueue;
    private IRulesetManager _rulesetManager;

    public BeatmapsController(LazerContext context, IBeatmapSetResolver resolver, IUserStorage storage, IBackgroundTaskQueue taskQueue, IRulesetManager rulesetManager)
    {
        _context = context;
        _resolver = resolver;
        _storage = storage;
        _taskQueue = taskQueue;
        _rulesetManager = rulesetManager;
    }

    [HttpGet("/api/v2/beatmaps/lookup")]
    [RequiredLazerClient]
    public async Task<IActionResult> LookUpBeatmapSetAsync([FromQuery(Name = "id")] int id, [FromQuery(Name = "checksum")] string? checksum)
    {
        var beatmap = await _resolver.FetchBeatmap(id);

        if (beatmap is null)
            return NotFound();

        return Json(await beatmap.ToOsu());
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
    public async Task<IActionResult> GetScoresAsync([FromServices] ILogger<BeatmapsController> logger, [FromServices] ILeaderboardManager manager, [FromRoute(Name = "id")] int beatmapId, [FromQuery(Name = "type")] string type, [FromQuery(Name = "mode")] string mode)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        
        var beatmap = await _resolver.FetchBeatmap(beatmapId);



        if (beatmap is null)
        {
            logger.LogWarning($"Beatmap {beatmapId} not found.");
            return NotFound(new { error = "No leaderboard found" });
        }

        var ruleset = await _rulesetManager.GetRulesetByName(mode);
        var leaderboardType = (BeatmapLeaderboardType) Enum.Parse(typeof(BeatmapLeaderboardType), char.ToUpper(type[0]) + type.Substring(1));
        var leaderboard = await manager.GetBeatmapLeaderboard(beatmapId, leaderboardType, leaderboardType == BeatmapLeaderboardType.Country ? user.Country : null, ruleset.RulesetInfo);

        if (leaderboard is null)
        {
            logger.LogWarning($"Leaderboard {beatmapId} is null.");
  
            return NotFound(new { error = "No leaderboard found" });
        }

        return Json(new ScoresResponse
        {
            Scores = (await Task.WhenAll(leaderboard.Select(c => c.ToOsuScore(_resolver)))).ToList(),
            UserScore = await manager.GetUserScore(beatmapId, user.Id)
        });
    }
    
    [HttpPost("solo/scores")]
    [RequiredLazerClient]
    public async Task<IActionResult> CreateSubmissionToken([FromRoute(Name = "id")] int beatmapId)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var scoreToken = _storage.ScoreTokens.Any() ? _storage.ScoreTokens.Max(c => c.Key) + 1: 1 ;
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
    public async Task<IActionResult> SubmitScoreByToken([FromServices] ILeaderboardManager manager, [FromRoute(Name = "submitid")] int submitionToken, [FromRoute(Name = "id")] int beatmapId, [FromBody] APIScoreBody body)
    {
        var clock = new StopwatchClock(true);
        var score = await manager.ProcessScoreAsync(submitionToken, body, beatmapId, 0, 0);

        if (score is null)
            return BadRequest();
        
        clock.Stop();
        Console.WriteLine($"Submission finished in {clock.Elapsed.TotalSeconds}");
        return Json(score);
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
    public async Task<IActionResult> SubmitScore([FromServices] ILeaderboardManager manager, [FromRoute(Name = "roomId")] int roomId, [FromRoute(Name = "playlistItem")] int playlistItemId, [FromRoute(Name = "submitionToken")] int submitionToken, [FromBody] APIScoreBody body)
    {
        var playlistItem = _storage.PlaylistItems.Values.FirstOrDefault(p => p.ID == playlistItemId);
        
        if (playlistItem is null)
            return BadRequest();
        
        var score = await manager.ProcessScoreAsync(submitionToken, body, playlistItem.BeatmapId, roomId, playlistItemId);

        if (score is null)
            return BadRequest();
        
        return Json(score);
    }
}