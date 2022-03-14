using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Users;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;

namespace OsuLazerServer.Controllers;


[ApiController]
[Route("/api/v2/me")]
public class MeController : Controller
{


    private IUserStorage _storage;
    private LazerContext _context;


    public MeController(IUserStorage storage, LazerContext context)
    {
        _storage = storage;
        _context = context;
    }


    
    [Authorization]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        await user.FetchUserStats(_context);
        
        var stats = ModeUtils.GetStatsByMode("osu", user);
        return Json(await user.ToOsuUser("osu", _context));
    }

    [Authorization]
    [HttpGet("/api/v2/friends")]
    public async Task<IActionResult> GetFriends()
    {
        return Json(new List<APIUser>());
    }
}