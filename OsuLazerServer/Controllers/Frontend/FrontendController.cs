using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace OsuLazerServer.Controllers.Frontend;

public class FrontendController : Controller        
{
    
    public IActionResult Index()
    {
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot", "index.html"), MediaTypeNames.Text.Html);
    }
}