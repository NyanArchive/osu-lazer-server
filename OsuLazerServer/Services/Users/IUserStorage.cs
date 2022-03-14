using osu.Game.Scoring;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;

namespace OsuLazerServer.Services.Users;

public interface IUserStorage
{
    public Dictionary<string, User> Users { get; set; }
    
    public Dictionary<long, User> ScoreTokens { get; set; }
    
    public Dictionary<int, List<DbScore>> LeaderboardCache { get; set; }
    
}