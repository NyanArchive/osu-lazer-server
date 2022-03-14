using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Utils;


public enum RulesetId
{
    [EnumMember(Value = "osu")]
    Osu = 0,
    [EnumMember(Value = "taiko")]
    Taiko = 1,
    [EnumMember(Value = "fruits")]
    Fruits = 2,
    [EnumMember(Value = "mania")]
    Mania = 3
}
public class ModeUtils
{
    public static IUserStats GetStatsByMode(string mode, User user)
    {
        if (mode == "osu")
            return user.StatsOsu;
        if (mode == "taiko")
            return user.StatsTaiko;
        if (mode == "fruits")
            return user.StatsFruits;
        return mode == "mania" ? user.StatsMania : user.StatsOsu;
    }
    
    public static async Task<IUserStats> FetchUserStats(LazerContext context, string mode, int id)
    {
        
        if (mode == "osu")
            return  await context.OsuStats.FirstAsync(s => s.Id == id);
        if (mode == "taiko")
            return  await context.TaikoStats.FirstAsync(s => s.Id == id);
        if (mode == "fruits")
            return await context.FruitsStats.FirstAsync(s => s.Id == id);
        return mode == "mania" ?  await context.ManiaStats.FirstAsync(s => s.Id == id) : await context.OsuStats.FirstAsync(s => s.Id == id);
    }
    
}