using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Services.Users;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Utils;

public class LeaderboardUtils
{

    private IUserStorage _stats;
    public LeaderboardUtils(IUserStorage stats)
    {
        _stats = stats;
    }
    public async Task<Dictionary<int, IUserStats>> GetLeaderboardForOsu()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        var users = await Task.WhenAll(context.OsuStats.ToList().Select(async c =>
        {
            c.PerformancePoints = (int) await _stats.GetUserPerformancePoints(c.Id, 0);
            return c;
        }));
        foreach (var stats in users.OrderByDescending(c =>c.PerformancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            
            if (stats.Stats.PerformancePoints == 0)
                continue;
            
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    
    public  async Task<Dictionary<int, IUserStats>> GetLeaderboardForTaiko()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        var users = await Task.WhenAll(context.OsuStats.ToList().Select(async c =>
        {
            c.PerformancePoints = (int) await _stats.GetUserPerformancePoints(c.Id, 1);
            return c;
        }));
        foreach (var stats in users.OrderByDescending(c =>c.PerformancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            
            if (stats.Stats.PerformancePoints == 0)
                continue;
            
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    public async Task<Dictionary<int, IUserStats>> GetLeaderboardForFruits()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        var users = await Task.WhenAll(context.OsuStats.ToList().Select(async c =>
        {
            c.PerformancePoints = (int) await _stats.GetUserPerformancePoints(c.Id, 2);
            return c;
        }));
        foreach (var stats in users.OrderByDescending(c =>c.PerformancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            
            if (stats.Stats.PerformancePoints == 0)
                continue;
            
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    public async Task<Dictionary<int, IUserStats>> GetLeaderboardForMania()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        var users = await Task.WhenAll(context.OsuStats.ToList().Select(async c =>
        {
            c.PerformancePoints = (int) await _stats.GetUserPerformancePoints(c.Id, 3);
            return c;
        }));
        foreach (var stats in users.OrderByDescending(c =>c.PerformancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            
            if (stats.Stats.PerformancePoints == 0)
                continue;
            
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    
}