using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Models.Multiplayer;
using OsuLazerServer.Models;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/api/v2/")]
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

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms([FromQuery(Name = "category")] string category)
    {
        if (_storage.Rooms.Count == 0)
        {
            var p = _storage.Users.ElementAt(0).Value.ToOsuUser("osu");
            var b = (await _resolver.FetchBeatmap(3045293)).ToOsu();
            var item = new PlaylistItem(b);
            item.ApiBeatmap = b;
            item.OnlineBeatmapId = b.OnlineID;
            item.OwnerID = p.Id;

            _storage.Rooms.Add(0, new Room
            {
                RoomID = 0,
                Host = p,
                Name = ":TF:",
                MaxAttempts = 100,
                Type = osu.Game.Online.Rooms.MatchType.HeadToHead,
                CurrentPlaylistItem = item,
                Playlist = new List<PlaylistItem>
                {
                    item
                },
                Category = osu.Game.Online.Rooms.RoomCategory.Normal,
                ChannelId = 1,
                ParticipantCount = 0,
                QueueMode = osu.Game.Online.Multiplayer.QueueMode.HostOnly,
                RecentParticipants = new List<APIUser>
                {
                    p
                }
            });
        }

        var j = JsonSerializer.Serialize(_storage.Rooms.Values.ToList());



        return Json(_storage.Rooms.Values.ToList());
    }
}