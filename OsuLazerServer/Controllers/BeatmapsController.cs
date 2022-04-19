using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OsuLazerServer.Attributes;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;

namespace OsuLazerServer.Controllers;


[ApiController]
[Route("/api/v2/beatmapsets")]
public class BeatmapsController : Controller
{
    private IBeatmapSetResolver _resolver;
  

    public BeatmapsController(IBeatmapSetResolver resolver)
    {
        _resolver = resolver;
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetBeatmapSets([FromServices] IUserStorage storage, [FromQuery(Name = "q")] string? query = "all", [FromQuery(Name = "s")] string? status = "leaderboard", [FromQuery(Name = "m")] string? mode = "0", [FromQuery(Name = "cursor[id]")] string? cursor = "0")
    {
        if (cursor == "0" || !storage.BeatmapCursorOffset.ContainsKey(cursor))
        {
            cursor = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
        
        storage.BeatmapCursorOffset.TryAdd(cursor, 0);
        var beatmaps = await _resolver.FetchSets(query == "all" ? "" : query, Convert.ToInt32(mode), storage.BeatmapCursorOffset[cursor], false, status == "all" ? "any" : status);
        storage.BeatmapCursorOffset[cursor] += 50;
        return Json(new { beatmapsets = beatmaps.Take(50).Select(b => b.ToBeatmapSet()), total = (beatmaps.Count + 50), cursor = new Dictionary<string, object>()
        {
            {"id", cursor}
        }});
    }
}