using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Utils;

public class LeaderboardUtils
{


    public static async Task<Dictionary<int, IUserStats>> GetLeaderboardForOsu()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        foreach (var stats in context.OsuStats.AsEnumerable().OrderByDescending(c => c.PerfomancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    
    public static  async Task<Dictionary<int, IUserStats>> GetLeaderboardForTaiko()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        foreach (var stats in context.TaikoStats.AsEnumerable().OrderByDescending(c => c.PerfomancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    public static async Task<Dictionary<int, IUserStats>> GetLeaderboardForFruits()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        foreach (var stats in context.FruitsStats.AsEnumerable().OrderByDescending(c => c.PerfomancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    public static async Task<Dictionary<int, IUserStats>> GetLeaderboardForMania()
    {
        var context = new LazerContext();


        var response = new Dictionary<int, IUserStats>();
        foreach (var stats in context.ManiaStats.AsEnumerable().OrderByDescending(c => c.PerfomancePoints).Select((c, i) => new { Position = i, Stats = c}))
        {
            response[stats.Position] = stats.Stats;
        }

        return response;
    }
    
}