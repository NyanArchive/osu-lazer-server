using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Services.Wiki;

namespace OsuLazerServer.Controllers.Wiki;


[ApiController]
[Route("/api/v2/wiki/{language}")]
public class WikiController : Controller
{

    private IWikiResolver _resolver;

    public WikiController(IWikiResolver resolver)
    {
        _resolver = resolver;
    }
    [HttpGet("{page}")]
    public async Task<IActionResult> GetPage([FromRoute(Name = "page")] string page)
    {

        var wiki = _resolver.GetWikiByPage(page);

        return Json(wiki);
    }
}