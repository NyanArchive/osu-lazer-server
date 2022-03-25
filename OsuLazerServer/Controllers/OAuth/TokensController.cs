using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NuGet.Packaging;
using osu.Game.Utils;
using OsuLazerServer.Database;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Models.Response.OAuth;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Utils;
using Sentry;

namespace OsuLazerServer.Controllers.OAuth;


[ApiController]
[Route("/oauth")]
public class TokensController : Controller
{

    
    private ITokensService _tokensService;
    private LazerContext _context;
    private IUserStorage _storage;
    private IDistributedCache _cache;

    public TokensController(ITokensService tokensService, LazerContext context, IUserStorage storage, IDistributedCache cache)
    {
        _tokensService = tokensService;
        _context = context;
        _storage = storage;
        _cache = cache;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return Ok();
    }
    [HttpPost("token")]
    public async Task<IActionResult> PostToken([FromForm] OauthTokenRequest body)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower().Replace(" ", "_") == body.Username.ToLower().Replace(" ", "_") ||
            u.Email == body.Username);

        if (user is null)
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: User not found.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }
        
        
        if (user.Banned)
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: User banned.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }

        if (!BCrypt.Net.BCrypt.Verify(body.Password, user.Password))
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: Invalid password.",
                Message = "Invalid username or password",
                ErrorIdentifier = "-1"
            });
        }


#if !DEBUG
        if (user.Country == "XX")
        {
            user.Country = await IPUtils.GetCountry(Request.Headers["X-Forwarded-For"].ToString().Split(", ").FirstOrDefault());
            await _context.SaveChangesAsync();
        }


        if (user.Country != (await IPUtils.GetCountry(Request.Headers["X-Forwarded-For"].ToString().Split(", ").FirstOrDefault())))
        {
            Response.StatusCode = 401;
            return Json(new OAuthError
            {
                Hint = "CSetlia: Please, contact support (001).",
                Message = "Please, contact support (001)",
                ErrorIdentifier = "-1"
            });
        }
#endif

        var token = _tokensService.GenerateToken();
        _storage.Users.Add(token.AccessToken, user);
        await _cache.SetAsync($"token:{token.AccessToken}", Encoding.UTF8.GetBytes(user.Id.ToString()));
        await _cache.SetAsync($"token:{token.AccessToken}:expires_at", Encoding.UTF8.GetBytes(token.ExpiresIn.ToString()));



        Task.Run(async () =>
        {
            Task.Delay(500);
            var channel = new Channel
            {
                Description = "PM",
                Icon = null,
                Messages = new List<Message>(),
                Moderated = false,
                Name = UserStorage.SystemSender.Username,
                Type = "PM",
                Users = new List<int> {UserStorage.SystemSender.Id, user.Id},
                ChannelId = 99912,
                LastMessageId = null,
                LastReadId = null,

            };
        
            channel.Messages.Add(new Message { Content = "Welcome to lazer server!\nDiscord server: https://discord.gg/p9BhPXHZWB\nGit: http://s2.zloserver.com:32333/dhcpcd9/osu-lazer-server\n(DM me in Discord to activate account.) ", Sender = UserStorage.SystemSender, Timetamp = DateTime.Now, ChannelId = 99912, MessageId = (int) DateTimeOffset.Now.ToUnixTimeSeconds() / 1000, SenderId = UserStorage.SystemSender.Id });
            await _storage.AddUpdate(user.Id, new Update
            {
                Channels = new List<Channel>() { channel },
                Messages = channel.Messages,
                UpdateRecievedAt = DateTimeOffset.Now
            });
            await _storage.ForceJoinChannel(user.Id, 1); //Join #osu channel.
        });
        return Json(token);
    }
    
}