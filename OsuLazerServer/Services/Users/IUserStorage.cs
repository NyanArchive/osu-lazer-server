using System.Collections.Concurrent;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Models.Multiplayer;
using OsuLazerServer.SpectatorClient;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Services.Users;

public interface IUserStorage
{
    public Dictionary<string, User> Users { get; set; }
    public Dictionary<long, User> ScoreTokens { get; set; }
    public Dictionary<int, List<DbScore>> LeaderboardCache { get; set; }
    public Dictionary<string, List<User>> GlobalLeaderboardCache { get; set; }
    public Dictionary<int, Room> Rooms { get; set; }
    public ConcurrentDictionary<int, List<Update>> Updates { get; set; }
    public Dictionary<int, Channel> Channels { get; set; }
    public Dictionary<int, SpectatorState> UserStates { get; set; }
    public Dictionary<int, MultiplayerRoom> HubRooms { get; set; }
    public Dictionary<int, PlaylistItem> PlaylistItems { get; set; }

    #region Channels
    public Task SendMessageToUser(User user, string message, bool isAction);
    public Task ForceJoinChannel(int userId, int channelId);
    public Channel GetChannel(int channelId, bool forceFetch = false);
    public Task<Channel?> GetChannelAsync(int channelId, LazerContext context, bool forceFetch = false);
    public Task<Message> SendMessageToChannel(Sender sender, Channel channel, string message, bool isAction = false);
    #endregion
    
    #region Leaderboard 
    User GetUser(string token);
    public Task<int> GetUserRank(int userId, int mode, bool forceFetch = false);
    public Task<double> GetUserPerformancePoints(int userId, int mode, bool forceFetch = false);
    public Task<double> GetUserHitAccuracy(int userId, int mode, bool forceFetch = false);
    public Task<double> UpdateRankings(string mode);
    public Task<double> UpdatePerformance(string mode, int userId, double peromance);
    public Task<Dictionary<int, IUserStats>> GetLeaderboard(int ruleset);
    public Task<double> UpdateHitAccuracy(string mode, int userId, double accuracy);
    #endregion

    #region Updates
    public Task NotifyUser(int userId, string message);
    public Task AddUpdate(int userId, Update update);
    public Task<List<Update>> GetUpdatesForUser(int userId);
    public Task ClearUpdatesForUser(int userId);
    #endregion


}
