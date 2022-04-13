using Microsoft.AspNetCore.SignalR;
using NuGet.Packaging;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Models.Multiplayer;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.SpectatorClient;
using Channel = OsuLazerServer.Models.Chat.Channel;
using PlaylistItem = OsuLazerServer.Models.Multiplayer.PlaylistItem;

namespace OsuLazerServer.Multiplayer;

//TODO: Rewire room variable to method.
public class MultiplayerHub : Hub<IMultiplayerClient>, IMultiplayerServer
{
    private IUserStorage _storage;
    private IBeatmapSetResolver _resolver;
    private Task _countdownTask;

    private User _user =>
        _storage.GetUser(Context.GetHttpContext().Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));

    public MultiplayerHub(IUserStorage storage, IBeatmapSetResolver resolver)
    {
        _storage = storage;
        _resolver = resolver;
    }

    private async Task UpdateRoomState()
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        await Clients.Group(GetGroupId(room.RoomID)).RoomStateChanged(room.State);
    }

    public async Task LeaveRoom()
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        if (room is null)
            return;


        if (room.Users.Count == 1)
        {
            var serverRoom = _storage.Rooms[(int) room.RoomID];
            _storage.Rooms.Remove(serverRoom.Id.Value);
            _storage.Channels.Remove(serverRoom.ChannelId);
            _storage.HubRooms.Remove((int) room.RoomID);
        }

        if (room.Host?.UserID == _user.Id)
        {
            room.Host = room.Users.FirstOrDefault(u => u.UserID != _user.Id);
        }

        room.Users.Remove(room.Users.FirstOrDefault(u => u.UserID == _user.Id));
        await Clients.Group(GetGroupId(room.RoomID)).UserLeft(new MultiplayerRoomUser(_user.Id));
        await Groups.RemoveFromGroupAsync(GetGroupId(room.RoomID), Context.ConnectionId);
    }

    public async Task TransferHost(int userId)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        if (room.Host.UserID != _user.Id)
            throw new InvalidOperationException("You are not host!");

        if (!room.Users.Any(u => u.UserID == userId))
            throw new InvalidOperationException("This user doesn't exists.");

        room.Host = room.Users.FirstOrDefault(u => u.UserID == userId);

        await Clients.Groups(GetGroupId(room.RoomID)).HostChanged(userId);
    }

    public async Task KickUser(int userId)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        if (_user.Id != room.Host.UserID)
            throw new InvalidOperationException("Cannot kick without host privileges");

        if (userId == _user.Id)
            throw new InvalidOperationException("Cannot kick current user.");


        await Clients.Groups(GetGroupId(room.RoomID)).UserKicked(new MultiplayerRoomUser(userId));

        room.Users.Remove(room.Users.FirstOrDefault(c => c.UserID == userId));
    }

    public async Task ChangeSettings(MultiplayerRoomSettings settings)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        var playlistId = room.Settings.PlaylistItemId;
        room.Settings = settings;
        room.Settings.PlaylistItemId = playlistId;

        await Clients.Group(GetGroupId(room.RoomID)).SettingsChanged(settings);
    }


    public async Task ChangeState(MultiplayerUserState newState)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        room.Users.FirstOrDefault(u => u.UserID == _user.Id).State = newState;

        if (_user.Id == room.Host.UserID)
            room.Host.State = newState;

        if (room.Users.All(c => c.State == MultiplayerUserState.Loaded || c.State == MultiplayerUserState.Spectating))
        {
            foreach (var user in room.Users)
            {
                user.State = user.State == MultiplayerUserState.Spectating
                    ? MultiplayerUserState.Spectating
                    : MultiplayerUserState.Playing;
                changeState(user.UserID,
                    user.State == MultiplayerUserState.Spectating
                        ? MultiplayerUserState.Spectating
                        : MultiplayerUserState.Playing);
            }

            await Clients.Group(GetGroupId(room.RoomID)).MatchStarted();

            room.State = MultiplayerRoomState.Playing;
        }


        if (room.Users.All(c =>
                c.State == MultiplayerUserState.FinishedPlay || c.State == MultiplayerUserState.Spectating))
        {
            foreach (var user in room.Users)
            {
                await changeState(user.UserID, MultiplayerUserState.Idle);
            }


            await Clients.Group(GetGroupId(room.RoomID)).ResultsReady();
            room.Playlist[0].PlayedAt = DateTimeOffset.Now;

            room.State = MultiplayerRoomState.Open;
            await UpdateRoomState();
            if (room.Playlist.Any(c => c.PlayedAt is null))
            {
                room.Settings.PlaylistItemId = room.Playlist.FirstOrDefault(c => c.PlayedAt is null).ID;
                await Clients.Group(GetGroupId(room.RoomID)).SettingsChanged(room.Settings);
            }
            else
            {
                var channel = _storage.Channels[_storage.Channels[(int) room.RoomID].ChannelId];
                foreach (var u in channel.Users)
                {
                    await _storage.AddUpdate(u, new Update
                    {
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Content = "No more items in playlist, please add one more or leave.",
                                Sender = UserStorage.SystemSender,
                                Timetamp = DateTime.Now,
                                ChannelId = channel.ChannelId,
                                MessageId = channel.Messages.Count + 1,
                                SenderId = UserStorage.SystemSender.Id
                            }
                        }
                    });
                }

                room.Settings.PlaylistItemId = 0;
                await Clients.Group(GetGroupId(room.RoomID)).SettingsChanged(room.Settings);
            }

            return;
        }

        await Clients.Group(GetGroupId(room.RoomID)).UserStateChanged(_user.Id, newState);
    }

    private async Task changeState(int userId, MultiplayerUserState newState)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));


        var user = room.Users.FirstOrDefault(c => c.UserID == userId);

        user.State = newState;
        await Clients.Group(GetGroupId(room.RoomID)).UserStateChanged(userId, newState);
    }

    public async Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        await Clients.Group(GetGroupId(room.RoomID)).UserBeatmapAvailabilityChanged(_user.Id, newBeatmapAvailability);
    }

    public async Task ChangeUserMods(IEnumerable<APIMod> newMods)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));


        var user = room.Users.FirstOrDefault(u => u.UserID == _user.Id);
        user.Mods = newMods;

        await Clients.Group(GetGroupId(room.RoomID)).UserModsChanged(user.UserID, user.Mods);
    }


    public async Task editPlaylist()
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(u => u.UserID == _user.Id));

        var serverRoom = _storage.Rooms[(int) room.RoomID];


        serverRoom.Playlist.Clear();
        serverRoom.Playlist.AddRange((await Task.WhenAll(room.Playlist.Select(async r => new PlaylistItem
        {
            Beatmap = await (await _resolver.FetchBeatmap(r.BeatmapID)).ToOsu()
        }))).ToList());
        serverRoom.DifficultyRange = new RoomDifficultyRange
        {
            Max = serverRoom.Playlist.Max(c => c.Beatmap.StarRating),
            Min = serverRoom.Playlist.Min(c => c.Beatmap.StarRating)
        };

        foreach (var item in serverRoom.Playlist)
        {
            if (!_storage.PlaylistItems.TryGetValue((int) item.ID, out var playlistItem))
            {
                _storage.PlaylistItems.Add(item.ID, playlistItem);
            }
        }

        if (room.Settings.PlaylistItemId == 0)
        {
            room.Settings.PlaylistItemId = room.Playlist.FirstOrDefault()?.ID ?? 0;
            await Clients.Group(GetGroupId(room.RoomID)).SettingsChanged(room.Settings);
        }
    }

    public async Task SendMatchRequest(MatchUserRequest matchRequest)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));
        var serverRoom = _storage.Rooms[(int) room.RoomID];

        if (matchRequest is StartMatchCountdownRequest request)
        {
            if (room.Host.UserID != _user.Id)
                throw new InvalidOperationException("Not host!");
            var countdown = new MatchStartCountdown
            {
                TimeRemaining = request.Duration
            };

            room.Countdown = countdown;
            serverRoom.StartCancellationTokenSource = new CancellationTokenSource();

            var currentGroup = Clients.Group(GetGroupId(room.RoomID));
            var internalCountdown = countdown.TimeRemaining;
            _countdownTask = new Task(async () =>
            {
                while (true)
                {
                    var token = _storage.Rooms[(int) room.RoomID].StartCancellationTokenSource;
                    if (token.IsCancellationRequested)
                        break;

                    internalCountdown = internalCountdown.Subtract(TimeSpan.FromSeconds(1));
                    var channel = _storage.Channels[_storage.Rooms[(int) room.RoomID].ChannelId];

                    if (internalCountdown.TotalSeconds <= 5 && internalCountdown.TotalSeconds > 0)
                    {
        
                        foreach (var user in channel.Users)
                        {
                            await _storage.AddUpdate(user, new Update
                            {
                                Channels = new List<Channel>(),
                                Messages = new List<Message>
                                {
                                    new Message
                                    {
                                        Content = $"Starting match in {internalCountdown.TotalSeconds} seconds!",
                                        Sender = UserStorage.SystemSender,
                                        Timetamp = DateTime.Now,
                                        ChannelId = channel.ChannelId,
                                        MessageId = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000
                                    }
                                }
                            });
                        }
                    }
                    if (internalCountdown.TotalSeconds <= 0)
                    {
                        foreach (var user in channel.Users)
                        {
                            await _storage.AddUpdate(user, new Update
                            {
                                Channels = new List<Channel>(),
                                Messages = new List<Message>
                                {
                                    new Message
                                    {
                                        Content = $"Starting match!",
                                        Sender = UserStorage.SystemSender,
                                        Timetamp = DateTime.Now,
                                        ChannelId = channel.ChannelId,
                                        MessageId = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000
                                    }
                                }
                            });
                        }
                        room.State = MultiplayerRoomState.WaitingForLoad;
        
      
                        foreach (var user in room.Users.Where(c => c.State == MultiplayerUserState.Ready))
                        {
                            await changeState(user.UserID, MultiplayerUserState.WaitingForLoad);
                        }
                        await Clients.Group(GetGroupId(room.RoomID)).RoomStateChanged(room.State);
                        await Clients.Group(GetGroupId(room.RoomID)).LoadRequested();
                        break;
                    }

                    await currentGroup.MatchEvent(new CountdownChangedEvent
                    {
                        Countdown = countdown
                    });
  

                    await Task.Delay(1000);
                }
            });
            _countdownTask.Start();
        }

        if (matchRequest is StopCountdownRequest)
        {
            if (room.Countdown is null)
                throw new InvalidOperationException("No countdown!");
            serverRoom.StartCancellationTokenSource.Cancel();
            room.Countdown = null;
            await Clients.Group(GetGroupId(room.RoomID)).MatchEvent(new CountdownChangedEvent
            {
                Countdown = null
            });
        }
    }

    public async Task StartMatch()
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        if (room.Host.UserID != _user.Id)
            throw new InvalidOperationException("You are not host!");

        if (room.Playlist.First().Expired)
            throw new InvalidOperationException("This playlist item is expired!");

        if (room.Users.Count(u => u.State == MultiplayerUserState.Ready) == 0)
            throw new InvalidOperationException("Cannot start with no ready users.");

        if (room.Host != null && room.Host.State != MultiplayerUserState.Spectating &&
            room.Host.State != MultiplayerUserState.Ready)
            throw new InvalidOperationException("Cannot start with not ready host.");


        room.State = MultiplayerRoomState.WaitingForLoad;
        foreach (var user in room.Users.Where(c => c.State == MultiplayerUserState.Ready))
        {
            await changeState(user.UserID, MultiplayerUserState.WaitingForLoad);
        }

        await Clients.Group(GetGroupId(room.RoomID)).LoadRequested();
    }

    public async Task AbortGameplay()
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        if (room.Host.UserID != _user.Id)
            throw new InvalidOperationException("You are not a host!");

        foreach (var u in room.Users)
        {
            await changeState(u.UserID, MultiplayerUserState.Idle);
        }


        room.State = MultiplayerRoomState.Open;
        await Clients.Group(GetGroupId(room.RoomID)).RoomStateChanged(room.State);
    }

    public async Task AddPlaylistItem(MultiplayerPlaylistItem item)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        item.ID = item.BeatmapID;
        item.OwnerID = _user.Id;
        room.Playlist.Add(item);

        await Clients.Group(GetGroupId(room.RoomID)).PlaylistItemAdded(item);

        await editPlaylist();
    }

    public async Task EditPlaylistItem(MultiplayerPlaylistItem item)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));

        item.ID = room.Playlist[0].ID;
        item.OwnerID = _user.Id;
        room.Playlist[0] = item;

        await Clients.Group(GetGroupId(room.RoomID)).PlaylistItemChanged(item);

        await editPlaylist();
    }

    public async Task RemovePlaylistItem(long playlistItemId)
    {
        var room = _storage.HubRooms.Values.FirstOrDefault(c => c.Users.Any(d => d.UserID == _user.Id));


        if (room.Playlist.Count < 2)
            throw new InvalidOperationException("Playlist cannot be empty!");


        room.Playlist.Remove(room.Playlist.FirstOrDefault(p => p.ID == playlistItemId));
        if (playlistItemId == room.Settings.PlaylistItemId)
        {
            room.Settings.PlaylistItemId = room.Playlist.FirstOrDefault().ID;
            await ChangeSettings(room.Settings);
        }

        await editPlaylist();
        await Clients.Group(GetGroupId(room.RoomID)).PlaylistItemRemoved(playlistItemId);
    }

    public Task<MultiplayerRoom> JoinRoom(long roomId) => JoinRoomWithPassword(roomId, String.Empty);

    public async Task<MultiplayerRoom> JoinRoomWithPassword(long roomId, string password)
    {
        if (_user.Banned)
            throw new InvalidStateException("Can't join a room when restricted.");
        if (_storage.HubRooms.Values.Any(c => c.Users.Any(u => u.UserID == _user.Id)))
            throw new InvalidStateException("Already joined to room.");
        if (!_storage.HubRooms.TryGetValue((int) roomId, out var room))
            throw new InvalidStateException("Room not found.");

        if (!string.IsNullOrEmpty(room.Settings.Password))
        {
            if (room.Settings.Password != password)
                throw new InvalidPasswordException();
        }

        room.Users.Add(new MultiplayerRoomUser(_user.Id));

        var channelId = _storage.Rooms[(int) roomId]?.ChannelId ?? 0;
        _storage.Channels[channelId].Users.Add(_user.Id);

        foreach (var uId in _storage.Channels[channelId].Users)
        {
            await _storage.AddUpdate(uId, new Update
            {
                Channels = new List<Channel> {},
                Messages = new List<Message>
                {
                    new Message
                    {
                        Content = $"> User @{_user.Username} joined!",
                        Sender = UserStorage.SystemSender,
                        Timetamp = DateTime.Now,
                        ChannelId = channelId,
                        MessageId = DateTimeOffset.Now.ToUnixTimeSeconds() / 1000,
                        SenderId = UserStorage.SystemSender.Id
                    }
                }
            });
        }

        await Clients.Group(GetGroupId(roomId)).UserJoined(new MultiplayerRoomUser(_user.Id));

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupId(roomId));


        return room;
    }

    public string GetGroupId(long id) => $"multi:{id}";
}