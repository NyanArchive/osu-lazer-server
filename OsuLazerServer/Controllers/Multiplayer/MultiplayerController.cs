using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Models.Multiplayer;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/api/v2/")]
public class MultiplayerController : Controller
{
    private IUserStorage _storage;
    private LazerContext _context;

    public MultiplayerController(IUserStorage storage, LazerContext context)
    {
        _storage = storage;
        _context = context;
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms([FromQuery(Name = "category")] string category)
    {
        if (_storage.Rooms.Count == 0)
        {
            _storage.Rooms.Add(0, new Room
            {
                RoomID = 0,
                Host = _storage.Users.ElementAt(0).Value.ToOsuUser("osu"),
                Name = ":TF:",
                MaxAttempts = 100,
                Type = osu.Game.Online.Rooms.MatchType.HeadToHead
            });
        }

        return Json(_storage.Rooms.Values.ToList());
    }
}