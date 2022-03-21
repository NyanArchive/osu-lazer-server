using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Models.Multiplayer;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Chat;
using ItemAttemptsCount = OsuLazerServer.Models.Multiplayer.ItemAttemptsCount;
using MatchType = osu.Game.Online.Rooms.MatchType;
using PlaylistAggregateScore = OsuLazerServer.Models.Multiplayer.PlaylistAggregateScore;
using Room = OsuLazerServer.Models.Multiplayer.Room;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/api/v2/rooms")]
public class MultiplayerController : Controller
{
    private IUserStorage _storage;
    private LazerContext _context;
    private IBeatmapSetResolver _resolver;

    public MultiplayerController(IUserStorage storage, IBeatmapSetResolver resolver, LazerContext context)
    {
        _storage = storage;
        _context = context;
        _resolver = resolver;
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms([FromQuery(Name = "category")] string category)
    {
        return Json(_storage.Rooms.Values.ToList());
    }


    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {

        room.Playlist =
            (await Task.WhenAll(room.Playlist.Select(async c =>
            {
                c.ID = c.BeatmapId;
                c.OwnerId = room.Host.Id;
                c.Beatmap = await (await _resolver.FetchBeatmap(c.BeatmapId)).ToOsu(_resolver);
                return c;
            }))).ToList();
        room.PlaylistItemStats = new RoomPlaylistItemStats
        {
            CountActive = 0,
            CountTotal = 1,
            RulesetIDs = new [] { 0 }
        };
        
        //Adding playlist to memory
        foreach (var item in room.Playlist)
        {
            _storage.PlaylistItems.Add(item.ID, item);
        }
        
        room.Id = (int) (DateTimeOffset.Now.ToUnixTimeSeconds() / 1000);
        var channel = new Channel
        {
            Description = $"{room.Host.Username}'s multi room!",
            Icon = null,
            Messages = new List<Message>(),
            Moderated = false,
            Name = "multi",
            Type = "public",
            Users = new List<int> { room.Host.Id, 1 },
            ChannelId = (int) (DateTimeOffset.Now.ToUnixTimeSeconds() / 1000),
            LastMessageId = null,
            LastReadId = null
        };
        _storage.Channels.TryAdd(channel.ChannelId, channel);
        room.ChannelId = channel.ChannelId;
        room.DifficultyRange = new RoomDifficultyRange
        {
            Max = room.Playlist.Min(c => c.Beatmap?.StarRating??0),
            Min = room.Playlist.Max(c => c.Beatmap?.StarRating??0),
        };
        room.CurrentUserScore = new PlaylistAggregateScore
        {
            PlaylistItemAttempts = new ItemAttemptsCount[] { }
        };
        room.EndsAt = DateTime.Now.Add(TimeSpan.FromDays(1));
        room.Active = true;
        room.UserId = room.Host.Id;
        room.CurrentPlaylistItem = room.Playlist.FirstOrDefault();


        var hubRoom = new MultiplayerRoom((long)room.Id)
        {
            Host = new MultiplayerRoomUser(room.Host.Id),
            Playlist = room.Playlist.Select(c => new MultiplayerPlaylistItem { Expired = c.Expired, AllowedMods = c.AllowedMods, BeatmapChecksum = c.Beatmap.Checksum, ID = c.ID, PlayedAt = c.PlayedAt, PlaylistOrder = c.PlaylistOrder.GetValueOrDefault(), RequiredMods = c.RequiredMods, BeatmapID = c.BeatmapId, OwnerID = c.OwnerId, RulesetID = c.RulesetId}).ToList(),
            Settings = new MultiplayerRoomSettings
            {
                Name = room.Name,
                Password = room.Password,
                MatchType = MatchType.HeadToHead,
                QueueMode = QueueMode.HostOnly,
                PlaylistItemId = room.Playlist.First().ID
            },
            State = MultiplayerRoomState.Open,
            Users = new List<MultiplayerRoomUser>(),
            MatchState = null,
        };
        _storage.HubRooms.TryAdd(room.Id.Value, hubRoom);
        _storage.Rooms.TryAdd(room.Id.Value, room);
        
        return Json(room);
    }

    [HttpPut("{roomId:int}/users/{userId}")]
    [RequiredLazerClient]
    public async Task<IActionResult> JoinRoomAsync([FromRoute(Name = "roomId")] int roomId, [FromRoute(Name = "userId")] int userId)
    {
        var user = _storage.Users.Values.FirstOrDefault(u => u.Id == userId);
        if (!_storage.Rooms.TryGetValue(roomId, out var room))
            return NotFound();
        
        room.RecentParticipants.Add(user?.ToOsuUser("osu", _storage));
        return Ok();
    }
    [HttpDelete("{roomId:int}/users/{userId}")]
    [RequiredLazerClient]
    public async Task<IActionResult> LeaveRoomAsync([FromRoute(Name = "roomId")] int roomId, [FromRoute(Name = "userId")] int userId)
    {
        var user = _storage.Users.Values.FirstOrDefault(u => u.Id == userId);
        if (!_storage.HubRooms.TryGetValue(roomId, out var room))
            return NotFound();
        
        room.Users.Remove(room.Users.FirstOrDefault(c => c.UserID == user?.Id));
        return Ok();
    }
}