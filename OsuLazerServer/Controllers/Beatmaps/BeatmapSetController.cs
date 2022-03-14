using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Database;
using OsuLazerServer.Services.Beatmaps;

namespace OsuLazerServer.Controllers.Beatmaps;


[ApiController]
[Route("/api/v2/beatmapsets/{setId:int}")]
public class BeatmapSetController : Controller
{

    private IBeatmapSetResolver _resolver;
    private LazerContext _context;

    public BeatmapSetController(IBeatmapSetResolver resolver, LazerContext context)
    {
        _resolver = resolver;
        _context = context;
    }
    public async Task<IActionResult> Index([FromRoute(Name = "setId")] int setId)
    {
        var beatmapset = await _resolver.FetchSetAsync(setId);

        if (beatmapset is null)
            return NotFound();

        return Json(beatmapset.ToBeatmapSet());
    }
    
    [HttpGet("download")]
    public async Task<IActionResult> DownloadBeatmapSet([FromRoute(Name = "setId")] int setId)
    {
        return Redirect($"https://api.nerina.pw/d/{setId}");
    }
    
    
    
}