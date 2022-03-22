using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.SpectatorClient;

public class SpectatorHub : Hub<ISpectatorClient>, ISpectatorServer
{
    private IUserStorage _storage;
    private ILogger<SpectatorHub> _logger;
    private User? _user =>
        _storage.GetUser(Context.GetHttpContext().Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));

    public SpectatorHub(IUserStorage storage, ILogger<SpectatorHub> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (_user is null)
        {
            return;
            Context.Abort();
        }
        _storage.UserStates.TryAdd(_user.Id, new SpectatorState
        {
            Mods = new List<APIMod>(),
            State = SpectatedUserState.Idle,
            BeatmapID = 0,
            RulesetID = 0
        });

        foreach (var kvp in _storage.UserStates.Where(c => c.Value.State != SpectatedUserState.Idle))
            await Clients.Caller.UserBeganPlaying(kvp.Key, kvp.Value);
        await base.OnConnectedAsync();
    }



    public async Task BeginPlaySession(SpectatorState state)
    {
        _storage.UserStates.FirstOrDefault(s => s.Key == _user.Id).Value.State = SpectatedUserState.Playing;
        await Clients.All.UserBeganPlaying(_user.Id, state);
    }
    

    public async  Task SendFrameData(FrameDataBundle data)
    {
        await Clients.Group(GetGroupId(_user.Id)).UserSentFrames(_user.Id, data);
    }

    public async Task EndPlaySession(SpectatorState state)
    {
        
        _storage.UserStates.FirstOrDefault(s => s.Key == _user.Id).Value.State = SpectatedUserState.Idle;
        await Clients.All.UserFinishedPlaying(_user.Id, state);
    }

    public async Task StartWatchingUser(int userId)
    {
        try
        {
            SpectatorState state = _storage.UserStates[userId];

            await Clients.Caller.UserBeganPlaying(userId, state);
        }
        catch (KeyNotFoundException)
        {
            
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupId(userId));
    }

    public async Task EndWatchingUser(int userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupId(userId));
    }
    
    public static string GetGroupId(int userId) => $"watch:{userId}";
}