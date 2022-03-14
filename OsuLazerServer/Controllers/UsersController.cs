using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Users;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/users")]
public class UsersController : Controller
{

    private ITokensService _tokensService;
    private LazerContext _context;

    public UsersController(LazerContext context, ITokensService tokensService)
    {
        _tokensService = tokensService;
        _context = context;
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

        var osuUser = await user.ToOsuUser(mode, _context);
        return Json(osuUser);
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
            Country = "UA",
            PlayCount = 0,
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
}