using Microsoft.AspNetCore.Mvc;
using osu.Framework.Extensions.IEnumerableExtensions;
using OsuLazerServer.Database;
using OsuLazerServer.Models.Rankings;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Controllers;


[ApiController]
[Route("/api/v2/rankings")]
public class LeaderboardController : Controller
{

    private IUserStorage _storage;
    private LazerContext _context;

    public LeaderboardController(IUserStorage storage, LazerContext ctx)
    {
        _storage = storage;
        _context = ctx;
    }
    
    [HttpGet("{mode}/performance")]
    
    public async Task<IActionResult> GetRankingPerfomance([FromRoute(Name = "mode")] string mode, [FromQuery(Name = "page")] int page) 
    {

        if (!_storage.GlobalLeaderboardCache.TryGetValue(mode, out var cachedLeaderboard))
        {
            //Caclulating...
            var leaderboard = _context.Users.AsEnumerable().OrderByDescending(d => (d.FetchStats(mode))?.PerfomancePoints??0).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add(mode, leaderboard);

        

            return Json(new RankingResponse
            {
                Rankings = leaderboard.Select(u => RankingUser.FromUser(u.ToOsuUser(mode, new LazerContext()).Statistics, u.ToOsuUser(mode, new LazerContext()))).ToList(),
                Total = leaderboard.Count
            });
        }

        return Json(new RankingResponse
        {
            Rankings = cachedLeaderboard.Select(u => RankingUser.FromUser(u.ToOsuUser(mode, new LazerContext()).Statistics, u.ToOsuUser(mode, new LazerContext()))).ToList(),
            Total = cachedLeaderboard.Count
        });

    }
}