using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.SpectatorClient;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Services.Users;

public interface IUserStorage
{
    public Dictionary<string, User> Users { get; set; }
    
    public Dictionary<long, User> ScoreTokens { get; set; }
    
    public Dictionary<int, List<DbScore>> LeaderboardCache { get; set; }
    public Dictionary<string, List<User>> GlobalLeaderboardCache { get; set; }

    public Dictionary<int, Update> Updates { get; set; }
    
    public Dictionary<int, Channel> Channels { get; set; }
    
    public Dictionary<int, SpectatorState> UserStates { get; set; }

    public Task NotifyUser(int userId, string message);
    public Task AddUpdate(int userId, Update update);

    public Task<Update> GetUpdatesForUser(int userId);

    public Task<Channel> GetChannelAsync(int channelId, LazerContext context, bool forceFetch = false);
    

}
