using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Services.Replays;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.SpectatorClient;

public class SpectatorHub : Hub<ISpectatorClient>, ISpectatorServer
{
    private IUserStorage _storage;
    private ILogger<SpectatorHub> _logger;
    private IReplayManager _replayManager;
    private User? _user =>
        _storage.GetUser(Context.GetHttpContext().Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));

    public SpectatorState State => _storage.UserStates[_user.Id];


    public SpectatorHub(IUserStorage storage, ILogger<SpectatorHub> logger, IReplayManager replayManager)
    {
        _storage = storage;
        _logger = logger;
        _replayManager = replayManager;
    }

    public override async Task OnConnectedAsync()
    {
        if (_user is null)
        {
            Context.Abort();
            return;
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
        _replayManager.ClearReplayFrames(_user.Id);
        _storage.UserStates[_user.Id] = state;
        await Clients.All.UserBeganPlaying(_user.Id, state);
    }
    
    public async  Task SendFrameData(FrameDataBundle data)
    {
        await _replayManager.WriteReplayData(_user.Id, State.BeatmapID??0, data, State.Mods.ToList());
        await Clients.Group(GetGroupId(_user.Id)).UserSentFrames(_user.Id, data);
    }

    public async Task EndPlaySession(SpectatorState state)
    {
        
        _storage.UserStates[_user.Id] = state;
        await Clients.All.UserFinishedPlaying(_user?.Id??0, state);
    }

    public async Task StartWatchingUser(int userId)
    {
        try
        {
            SpectatorState state = _storage.UserStates[userId];

            if (state.State == SpectatedUserState.Playing)
                await Clients.Caller.UserBeganPlaying(userId, state);
        }
        catch (KeyNotFoundException)
        {
            
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupId(userId));
    }

    public async Task EndWatchingUser(int userId)
    {
        _replayManager.ClearReplayFrames(_user.Id);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupId(userId));
    }
    
    public static string GetGroupId(int userId) => $"watch:{userId}";
}