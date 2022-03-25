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
        {//
            //Caclulating...
            var leaderboard = (await _storage.GetLeaderboard(mode switch
            {
                "osu" => 0,
                "taiko" => 1,
                "fruits" => 2,
                "mania" => 3
            })).Select(c => _context.Users.FirstOrDefault(cc => c.Value.Id == cc.Id && !cc.Banned)).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:perfomance", leaderboard);
            

            return Json(new RankingResponse
            {
                Rankings = (await Task.WhenAll(leaderboard.Skip((page - 1) * 50).ToList().Where(c => c is not null).Select(async u => await RankingUser.FromUser(await u.ToOsuUser(mode, _storage), _storage, mode switch
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
            Rankings = (await Task.WhenAll(cachedLeaderboard.Where(u => u is not null).Select(async u => await RankingUser.FromUser(await u.ToOsuUser(mode, _storage), _storage, mode switch
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
            var leaderboard = _context.Users.AsEnumerable().OrderByDescending(d => (d.FetchStats(mode))?.RankedScore??0).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:score", leaderboard);
            

            return Json(new RankingResponse
            {
                Rankings = (await Task.WhenAll(leaderboard.Select(async u => await RankingUser.FromUser(await u.ToOsuUser(mode, _storage), _storage, mode switch
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
            Rankings = (await Task.WhenAll(cachedLeaderboard.Select(async u => await RankingUser.FromUser(await u.ToOsuUser(mode, _storage), _storage, mode switch
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

        var rulesetId = mode switch
        {
            "osu" => 0,
            "taiko" => 1,
            "fruits" => 2,
            "mania" => 3
        };
        if (!_storage.GlobalLeaderboardCache.TryGetValue($"{mode}:perfomance", out var cachedLeaderboard))
        {
            //Caclulating...
            var leaderboard = (await _storage.GetLeaderboard(rulesetId)).Select(c => _context.Users.Where(u => !u.Banned).FirstOrDefault(cc => cc.Id == c.Value.Id)).Skip((page - 1) * 50).ToList();
            _storage.GlobalLeaderboardCache.Add($"{mode}:perfomance", leaderboard);

            var countries = new List<RankingCountry>();

            foreach (var u in leaderboard)
            {
                
                var ranking = new RankingCountry();
                var stats = u.FetchStats(mode);
                var pp = await _storage.GetUserPerformancePoints(stats.Id, rulesetId);
                if (!countries.Any(c => c.Country.FlagName == u.Country))
                {
                    ranking = new RankingCountry
                    {
                        Code = u.Country,
                        Country = new Country
                        {
                            FlagName = u.Country,
                            FullName =  new CountryProvider()?.GetCountry(u.Country)?.CommonName??"Unknown"
                        },
                        Performance = (int)pp,
                        ActiveUsers = 1,
                        PlayCount = u.PlayCount,
                        RankedScore = (int) (stats?.RankedScore??0)
                    };
                    
                    countries.Add(ranking);
                    continue;
                }
                var userStats = u.FetchStats(mode);

                ranking = countries.FirstOrDefault(c => c.Code == u.Country);

                ranking.Performance += (int)pp;
                ranking.PlayCount += u.PlayCount;
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
            var stats = u.FetchStats(mode);
            var pp = await _storage.GetUserPerformancePoints(stats.Id, rulesetId);
            if (!cachedCountries.Any(c => c.Country.FlagName == u.Country))
            {
                ranking = new RankingCountry
                {
                    Code = u.Country,
                    Country = new Country
                    {
                        FlagName = u.Country,
                        FullName =  new CountryProvider()?.GetCountry(u.Country)?.CommonName??"Unknown"
                    },
                    Performance = (int)pp,
                    ActiveUsers = 1,
                    PlayCount = u.PlayCount,
                    RankedScore = (int) (stats?.RankedScore??0)
                };
                    
                cachedCountries.Add(ranking);
                continue;
            }
            var userStats = u.FetchStats(mode);

            ranking = cachedCountries.FirstOrDefault(c => c.Code == u.Country);

            ranking.Performance += (int)pp;
            ranking.PlayCount += u.PlayCount;
            ranking.ActiveUsers++;
            ranking.RankedScore += (int) (userStats?.RankedScore??0);
        }
        return Json(new RankingCountryResponse
        {
            Rankings = cachedCountries,
            Total = cachedLeaderboard.Count
        });
    }
}