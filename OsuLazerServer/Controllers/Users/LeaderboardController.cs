using Microsoft.AspNetCore.Mvc;
using Nager.Country;
using osu.Framework.Extensions.IEnumerableExtensions;
using OsuLazerServer.Database;
using OsuLazerServer.Models.Rankings;
using OsuLazerServer.Models.Response.Users;
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
        if (!_storage.GlobalLeaderboardCache.TryGetValue($"{mode}:perfomance", out var cachedLeaderboard))
        {
            //Caclulating...
            var leaderboard = _context.Users.AsEnumerable().OrderByDescending(d => (d.FetchStats(mode))?.PerfomancePoints??0).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:perfomance", leaderboard);
            

            return Json(new RankingResponse
            {
                Rankings = (await Task.WhenAll(leaderboard.Select(async u => await RankingUser.FromUser(u.ToOsuUser(mode).Statistics, u.ToOsuUser(mode), _storage, mode switch
                {
                    "osu" => 0,
                    "taiko" => 1,
                    "fruits" => 2,
                    "mania" => 3
                })))).ToList(),
                Total = leaderboard.Count
            });
        }

        return Json(new RankingResponse
        {
            Rankings = (await Task.WhenAll(cachedLeaderboard.Select(async u => await RankingUser.FromUser(u.ToOsuUser(mode).Statistics, u.ToOsuUser(mode), _storage, mode switch
            {
                "osu" => 0,
                "taiko" => 1,
                "fruits" => 2,
                "mania" => 3
            })))).ToList(),
            Total = cachedLeaderboard.Count
        });
    }
    
    
    [HttpGet("{mode}/score")]
    
    public async Task<IActionResult> GetRankingScore([FromRoute(Name = "mode")] string mode, [FromQuery(Name = "page")] int page) 
    {

        if (!_storage.GlobalLeaderboardCache.TryGetValue($"{mode}:score", out var cachedLeaderboard))
        {
            //Caclulating...
            var leaderboard = _context.Users.AsEnumerable().OrderByDescending(d => (d.FetchStats(mode))?.TotalScore??0).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:score", leaderboard);
            

            return Json(new RankingResponse
            {
                Rankings = (await Task.WhenAll(leaderboard.Select(async u => await RankingUser.FromUser(u.ToOsuUser(mode).Statistics, u.ToOsuUser(mode), _storage, mode switch
                {
                    "osu" => 0,
                    "taiko" => 1,
                    "fruits" => 2,
                    "mania" => 3
                })))).ToList(),
                Total = leaderboard.Count
            });
        }

        return Json(new RankingResponse
        {
            Rankings = (await Task.WhenAll(cachedLeaderboard.Select(async u => await RankingUser.FromUser(u.ToOsuUser(mode).Statistics, u.ToOsuUser(mode), _storage, mode switch
            {
                "osu" => 0,
                "taiko" => 1,
                "fruits" => 2,
                "mania" => 3
            })))).ToList(),
            Total = cachedLeaderboard.Count
        });
    }
    
    [HttpGet("{mode}/country")]
    
    public async Task<IActionResult> GetRankingsCountry([FromRoute(Name = "mode")] string mode, [FromQuery(Name = "page")] int page) 
    {

        if (!_storage.GlobalLeaderboardCache.TryGetValue($"{mode}:perfomance", out var cachedLeaderboard))
        {
            //Caclulating...
            var leaderboard = _context.Users.AsEnumerable().OrderByDescending(d => (d.FetchStats(mode))?.PerfomancePoints??0).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:perfomance", leaderboard);

            var countries = new List<RankingCountry>();

            foreach (var u in leaderboard)
            {
                var ranking = new RankingCountry();
                if (!countries.Any(c => c.Country.FlagName == u.Country))
                {
                    var stats = u.FetchStats(mode);
                    ranking = new RankingCountry
                    {
                        Code = u.Country,
                        Country = new Country
                        {
                            FlagName = u.Country,
                            FullName =  new CountryProvider().GetCountry(u.Country).CommonName
                        },
                        Performance = stats?.PerfomancePoints??0,
                        ActiveUsers = 1,
                        PlayCount = u.PlayCount,
                        RankedScore = (int) (stats?.RankedScore??0)
                    };
                    
                    countries.Add(ranking);
                    continue;
                }
                var userStats = u.FetchStats(mode);

                ranking = countries.FirstOrDefault(c => c.Code == u.Country);

                ranking.Performance += userStats?.PerfomancePoints??0;
                ranking.PlayCount += u?.PlayCount??0;
                ranking.ActiveUsers++;
                ranking.RankedScore += (int) (userStats?.RankedScore??0);
            }
            

            return Json(new RankingCountryResponse
            {
                Rankings = countries,
                Total = leaderboard.Count
            });
        }

        
        var cachedCountries = new List<RankingCountry>();

        foreach (var u in cachedLeaderboard)
        {
            var ranking = new RankingCountry();
            if (!cachedCountries.Any(c => c.Country.FlagName == u.Country))
            {
                var stats = u.FetchStats(mode);
                ranking = new RankingCountry
                {
                    Code = u.Country,
                    Country = new Country
                    {
                        FlagName = u.Country,
                        FullName =  new CountryProvider().GetCountry(u.Country).CommonName
                    },
                    Performance = stats?.PerfomancePoints??0,
                    ActiveUsers = 1,
                    PlayCount = u.PlayCount,
                    RankedScore = (int) (stats?.RankedScore??0)
                };
                    
                cachedCountries.Add(ranking);
                continue;
            }
            var userStats = u.FetchStats(mode);
            
            if (userStats is null)
                continue;
            ranking = cachedCountries.First(c => c.Code == u.Country);

            ranking.Performance += userStats.PerfomancePoints;
            ranking.PlayCount += u.PlayCount;
            ranking.ActiveUsers++;
            ranking.RankedScore += (int) userStats.RankedScore;
        }
        return Json(new RankingCountryResponse
        {
            Rankings = cachedCountries,
            Total = cachedLeaderboard.Count
        });
    }
}