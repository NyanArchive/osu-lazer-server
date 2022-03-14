using osu.Game.Scoring;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using UniqueIdGenerator.Net;

namespace OsuLazerServer.Services.Users;

public class UserStorage : IUserStorage
{
    public Dictionary<string, User> Users { get; set; } = new ();
    public Dictionary<long, User> ScoreTokens { get; set; } = new();
    public Dictionary<int, List<DbScore>> LeaderboardCache { get; set; } = new();
}