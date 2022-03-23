using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Attributes;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Services.Commands;
using OsuLazerServer.Services.Users;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Controllers.Channels;


[ApiController]
[Route("/api/v2/chat")]
public class ChannelsController : Controller
{


    private IUserStorage _storage;
    private LazerContext _context;
    private ICommandManager _commands;

    public ChannelsController(IUserStorage storage, LazerContext context, ICommandManager manager)
    {
        _storage = storage;
        _context = context;
        _commands = manager;
    }

    [HttpGet("channels")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetChannels()
    {
        return Json(_context.Channels.Where(u => true).Select(c => new Channel
        {
            Description = c.Description,
            Icon = null,
            Moderated = false,
            Name = c.Name,
            Type = c.Type == ChannelType.PM ? "PM" : "PUBLIC",
            Users = new List<int>(),
            ChannelId = c.Id,
            LastMessageId = null,
            LastReadId = null,
        }).ToList());
    }

    [HttpGet("updates")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetUpdates([FromQuery(Name = "since")] ulong since)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        
        
        var updates = await _storage.GetUpdatesForUser(user.Id);

        Task.Run(async () =>
        {
            await Task.Delay(500);
            _storage.Updates[user.Id].Channels.Clear();
            _storage.Updates[user.Id].Messages.Clear();
        });

        return Json(updates);
    }
    [HttpPut("channels/{channelid}/users/{userId}")]
    [RequiredLazerClient]
    public async Task<IActionResult> PutUser([FromRoute(Name = "channelid")] int channelId, [FromRoute(Name = "userId")] int userId)
    {
         
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (channel.Users.Contains(user.Id))
            return Json(channel);
        
        channel.Users.Add(user.Id);
        await _storage.AddUpdate(user.Id, new Update
        {
            Channels = new List<Channel> {channel},
            Messages = channel.Messages
        });
        
        return Json(channel);
    }
    
    [HttpDelete("channels/{channelid}/users/{userId}")]
    [RequiredLazerClient]
    public async Task<IActionResult> LeaveUserFromChannel([FromRoute(Name = "channelid")] int channelId, [FromRoute(Name = "userId")] int userId)
    {
         
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        
        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (channel.Users is null)
            return NotFound();
        
        channel.Users.Remove(user.Id);
        
        return Ok();
    }

    [HttpGet("channels/{channelId:int}/messages")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetMessages([FromRoute(Name = "channelId")] int channelId)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (channel is null)
            return NotFound();
        if (!channel.Users.Contains(user.Id))
            return BadRequest();
        
        return Json(channel.Messages);
    }

    [HttpPost("channels/{channelId:int}/messages")]
    [RequiredLazerClient]
    public async Task<IActionResult> PostMessageToChannel([FromRoute(Name = "channelid")] int channelId, [FromForm] MessagePostBody body)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (!channel.Users.Contains(user.Id))
            return BadRequest();

        if (body.Message.StartsWith("!"))
        {
            var command = _commands.GetCommandByName(body.Message);

            if (command is null)
            {
                return Json(new Message
                {
                    Content = "Command not found.",
                    Sender = UserStorage.SystemSender,
                    Timetamp = DateTime.Now,
                    ChannelId = channel.ChannelId,
                    MessageId = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() / 1000) + new Random().Next(1, 30000),
                    SenderId = UserStorage.SystemSender.Id
                });
            }
            
            var result = command.Action.Invoke(body.Message.Split(" ").ToList().GetRange(1, body.Message.Split(" ").Length - 1));

            if (command.AdminRequired && !user.IsAdmin)
            {
                return Json(new Message
                {
                    Content = "No permissions.",
                    Sender = UserStorage.SystemSender,
                    Timetamp = DateTime.Now,
                    ChannelId =  channel.ChannelId,
                    MessageId = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() / 1000) + new Random().Next(1, 30000),
                    SenderId = UserStorage.SystemSender.Id
                }); 
            }
            return Json(new Message
            {
                Content = result,
                Sender = UserStorage.SystemSender,
                Timetamp = DateTime.Now,
                ChannelId = channel.ChannelId,
                MessageId = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() / 1000) + new Random().Next(1, 30000),
                SenderId = UserStorage.SystemSender.Id
            });
        }

        
        foreach (var uId in channel.Users.Where(c => c != user.Id))
        {
            var message = new Message
            {
                Content = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(body.Message)),
                Sender = new Sender
                {
                    Id = user.Id,
                    Username = user.Username,
                    AvatarUrl =
                        "https://media.discordapp.net/attachments/951904126164410388/953323388867334194/IMG_20220302_194306_647-removebg-preview.png",
                    CountryCode = user.Country,
                    DefaultGroup = "user",
                    IsActive = false,
                    IsBot = false,
                    IsDeleted = false,
                    IsOnline = true,
                    IsSupporter = false,
                    LastVisit = DateTime.Now,
                    ProfileColour = null,
                    PmFriendsOnly = false
                },
                Timetamp = DateTime.Now,
                ChannelId = channel.ChannelId,
                MessageId = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                SenderId = user.Id
            };
            _storage.Channels[channel.ChannelId].Messages.Add(message);
            await _storage.AddUpdate(uId, new Update
            {
                Channels = new List<Channel> {channel},
                Messages = new List<Message> {message}
            });
        }

        return Json(   new Message
        {
            Content = body.Message,
            Sender = new Sender
            {
                Id = user.Id,
                Username = user.Username,
                AvatarUrl = "https://media.discordapp.net/attachments/951904126164410388/953323388867334194/IMG_20220302_194306_647-removebg-preview.png",
                CountryCode = user.Country,
                DefaultGroup = "user",
                IsActive = true,
                IsBot = false,
                IsDeleted = false,
                IsOnline = true,
                IsSupporter = true,
                LastVisit = DateTime.Now,
                ProfileColour = "#FFFFFF",
                PmFriendsOnly = false
            },
            Timetamp = DateTime.Now,
            ChannelId = channel.ChannelId,
            MessageId = DateTimeOffset.Now.ToUnixTimeSeconds(),
            SenderId = user.Id
        });
    }

    [HttpPost("channels")]
    [RequiredLazerClient]
    public async Task<IActionResult> CreateChannelAsync([FromForm] CreatePMForm body)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        var target = _storage.Users.Values.FirstOrDefault(c => c.Id == body.TargetId);

        if (target is null)
            return NotFound();
        
        var channel = _storage.Channels.Values.FirstOrDefault(c => c.Type == "PM" && c.Users.Contains(body.TargetId) && c.Users.Contains(user.Id));
        if (channel is not null)
            return Json(new {channel_id = channel.ChannelId, recent_messages = channel.Messages, name = target.Username});


        var newChannel = new Channel
        {
            Description = "Private messages.",
            Icon = null,
            Messages = new List<Message>(),
            Moderated = false,
            Name = user.Username,
            Type = "PM",
            Users = new List<int> {target.Id, user.Id},
            ChannelId = (int) DateTimeOffset.Now.ToUnixTimeSeconds() / 1000,
            LastMessageId = null,
            LastReadId = null
        };
        
        _storage.Channels.Add(newChannel.ChannelId, newChannel);
        return Json(new {channel_id = newChannel.ChannelId, recent_messages = new List<object>()});
    }

    [HttpPost("new")]
    [RequiredLazerClient]
    public async Task<IActionResult> NewMessageAsync([FromForm] MessagePostBody body)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        var target = _storage.Users.Values.FirstOrDefault(c => c.Id == body.TargetId);
        
        if (target is null)
            return NotFound();
        
        var channel = _storage.Channels.Values.FirstOrDefault(c => c.Type == "PM" && c.Users.Contains(body.TargetId.Value) && c.Users.Contains(user.Id));

        if (channel is null)
            return NotFound();

        var message = new Message
        {
            Content = body.Message,
            Sender = new Sender
            {
                Id = user.Id,
                Username = user.Username,
                AvatarUrl =
                    "https://media.discordapp.net/attachments/951904126164410388/953323388867334194/IMG_20220302_194306_647-removebg-preview.png",
                CountryCode = user.Country,
                DefaultGroup = "user",
                IsActive = false,
                IsBot = false,
                IsDeleted = false,
                IsOnline = true,
                IsSupporter = false,
                LastVisit = DateTime.Now,
                ProfileColour = null,
                PmFriendsOnly = false
            },
            Timetamp = DateTime.Now,
            ChannelId = channel.ChannelId,
            MessageId = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000,
            SenderId = user.Id
        };

        await _storage.AddUpdate(user.Id, new Update
        {
            Channels = new List<Channel> {channel},
            Messages = new List<Message> {message}
        });

        return Json(message);
    }
}

