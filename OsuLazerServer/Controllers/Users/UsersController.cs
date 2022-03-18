using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Nager.Country;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Users;
using OsuLazerServer.Models.Spectator;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
using Realms;
using Country = OsuLazerServer.Models.Country;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/users")]
public class UsersController : Controller
{

    private ITokensService _tokensService;
    private LazerContext _context;
    private IUserStorage _storage;
    private IBeatmapSetResolver _resolver;

    public UsersController(LazerContext context, ITokensService tokensService, IUserStorage storage, IBeatmapSetResolver resolver)
    {
        _tokensService = tokensService;
        _context = context;
        _storage = storage;
        _resolver = resolver;
    }



    [HttpGet("/api/v2/users/{id}/")]
    [Authorization]
    public async Task<IActionResult> FetchUser([FromRoute(Name = "id")] string id, [FromQuery(Name = "key")] string key)
    {
        return await FetchUser(id, key, "osu");
    }


    [HttpGet("/api/v2/users/{id}/{mode}")]
    [Authorization]
    public async Task<IActionResult> FetchUser([FromRoute(Name = "id")] string id, [FromQuery(Name = "key")] string key, [FromRoute(Name = "mode")] string mode = "osu")
    {
        
        var user = _context.Users.AsEnumerable().FirstOrDefault(u => key == "id" ? u.Id == Convert.ToInt32(id) : u.UsernameSafe == id.ToString().ToLower().Replace(" ", "_"));

        if (user is null)
            return NotFound();

        var osuUser = user.ToOsuUser(mode);

        if (_storage.Users.Values.Any(c => c.Id == user.Id))
            osuUser.IsOnline = true;
        return Json(osuUser);
    }

    [HttpGet("/api/v2/users/{id}/scores/best")]
    [Authorization]
    public async Task<IActionResult> GetBestScores([FromRoute(Name = "id")] int id, [FromQuery(Name = "limit")] int limit, [FromQuery(Name = "offset")] int offset)
    {
        var scores = _context.Scores.Where(c => c.UserId == id && c.Status == DbScoreStatus.BEST).OrderByDescending(c => c.PerfomancePoints).Skip(offset).Take(limit);

        return Json(scores.Select(c => c.ToOsuScore(_resolver).GetAwaiter().GetResult()));
    }
    private async Task<IActionResult> GenerateRegistrationError(RegistrationRequestErrors.UserErrors errors)
    {
        Response.StatusCode = 401;
        return Json(new RegistrationError
        {
            FormError = new RegistrationRequestErrors
            {
                User = errors
            }
        });
    }
    [HttpPost]
    public async Task<IActionResult> Index([FromForm] RegistrationBody body)
    {
        await Task.Run(_context.CreateBot);
        if (_context.Users.AsEnumerable().Any(u => u.UsernameSafe == body.Username.ToLower().Replace(' ', '_') || body.Email == u.Email))
            return await GenerateRegistrationError(new RegistrationRequestErrors.UserErrors
            {
                Email = new[] {"Username of email already took"},
                Username = new[] {"Username or email already took"}
            });

        if (!Regex.IsMatch(body.Username, @"^[a-zA-Z0-9_-]{3,15}$"))
            return await GenerateRegistrationError(new RegistrationRequestErrors.UserErrors
            {
                Username = new [] {"Invalid username"}
            });

        if (!Regex.IsMatch(body.Email, @"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+"))
            return await GenerateRegistrationError(new RegistrationRequestErrors.UserErrors
            {
                Email = new[] {"Invalid email."}
            });

        var newUser = new User
        {
            Banned = false,
            Email = body.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(body.Password),
            Username = body.Username,
            NicknameHistory = new[] { "Stop putin" },
            Country = "US",
            ReplaysWatches = 0,
            StatsFruits = new UsersStatsFruits(),
            StatsMania = new UsersStatsMania(),
            StatsOsu = new UsersStatsOsu(),
            StatsTaiko = new UsersStatsTaiko(),
            JoinedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);


        await _context.SaveChangesAsync();
        return Ok();


    }


    [HttpGet("/api/v2/users/")]
    public async Task<IActionResult> GetUrl()
    {
        var uri = Request.QueryString.Value;
        
        var ids = uri.Split("?").Last().Split("&").Select(s => s.Replace("ids[]=", "")).Select(val => Convert.ToInt32(val));
        
        
        return Json(new {users = ids.Select(id => toSpectatorUser(id))});
    }

    private SpectatorUser toSpectatorUser(int id)
    {
        var user = _context.Users.FirstOrDefault(d => d.Id == id);

        return new SpectatorUser
        {
            Country = new Country
            {
                Code = user.Country,
                Name = new CountryProvider().GetCountry(user.Country).CommonName
            },
            CountryCode = user.Country,
            Cover = new Cover
            {
                Id = null,
                CustomUrl = "https://media.discordapp.net/attachments/805142641427218452/954414155539046430/FOG8Lr3VQAAgi7t.jpg",
                Url = "https://media.discordapp.net/attachments/805142641427218452/954414155539046430/FOG8Lr3VQAAgi7t.jpg"
            },
            DefaultGroup = "default",
            Groups = new List<object>(),
            Id = user.Id,
            IsActive = true,
            IsBot = false,
            IsDeleted = false,
            IsOnline = _storage.Users.Values.Any(d => d.Id == id),
            IsSupporter = true,
            LastVisit = DateTime.Now,
            PmFriendsOnly = false,
            ProfileColour = null,
            StatisticsRuleset = new Dictionary<string, Statistics>
            {
                {"fruits", ModeUtils.FetchUserStats(new LazerContext(), "fruits", user.Id).ToOsu("fruits")},
                {"mania", ModeUtils.FetchUserStats(new LazerContext(), "mania", user.Id).ToOsu("mania")},
                {"osu", ModeUtils.FetchUserStats(new LazerContext(), "osu", user.Id).ToOsu("osu")},
                {"taiko", ModeUtils.FetchUserStats(new LazerContext(), "taiko", user.Id).ToOsu("taiko")},
            },
            Username = user.Username
            
        };
    }
}