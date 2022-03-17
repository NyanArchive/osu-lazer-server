using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Services.Wiki;

namespace OsuLazerServer.Controllers.NewsController;

[ApiController]
[Route("/api/v2/news")]
public class NewsController : Controller
{


    private IWikiResolver _resolver;



    public NewsController(IWikiResolver resolver)
    {
        _resolver = resolver;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return NotFound();
    }
}