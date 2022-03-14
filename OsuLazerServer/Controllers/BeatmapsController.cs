using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Attributes;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;

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
    public async Task<IActionResult> GetBeatmapSets([FromQuery(Name = "q")] string? query = "all", [FromQuery(Name = "s")] string? status = "leaderboard")
    {
        var beatmaps = await _resolver.FetchSets(query == "all" ? "" : query, "-1", 0, false, status == "all" ? "any" : status);
        return Json(new { beatmapsets = beatmaps.Select(b => b.ToBeatmapSet()), total = beatmaps.Count});
    }
}