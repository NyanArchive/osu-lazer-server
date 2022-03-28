using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Models.News;
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
        var news = _resolver.ListOfNews();
        return Json(new NewsResponse
        {
            NewsPosts = news.Select(_resolver.GetNewsEntry),
            SidebarMetadata = new NewsSidebar
            {
                Years = news.Select(_resolver.GetNewsEntry).Select(c => c.PublishedAt.Year).Distinct().OrderByDescending(c => c).ToArray(),
                CurrentYear = DateTime.Now.Year,
                NewsPosts = news.Select(_resolver.GetNewsEntry)
            }
        });
    }
    
    [HttpGet("/home/news/{post}")]
    public async Task<IActionResult> GetNewsPost(string post)
    {

        if (!_resolver.ListOfNews().Any(c => _resolver.GetNewsEntry(c).Slug == post))
            return NotFound();
        
        var news = _resolver.GetNewsEntry(_resolver.ListOfNews().FirstOrDefault(c => _resolver.GetNewsEntry(c).Slug == post));
        return Json(news);
    }
}