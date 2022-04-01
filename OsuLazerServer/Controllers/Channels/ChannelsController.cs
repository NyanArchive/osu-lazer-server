using System.Text;
using BackgroundQueue;
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
    private IBackgroundTaskQueue _queue;

    public ChannelsController(IUserStorage storage, LazerContext context, ICommandManager manager,
        IBackgroundTaskQueue queue)
    {
        _storage = storage;
        _context = context;
        _commands = manager;
        _queue = queue;
    }

    [HttpGet("channels")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetChannels()
    {
        return Json(await Task.WhenAll(_context.Channels.AsEnumerable().Where(u => true).Select(async c => new Channel
        {
            Description = c.Description,
            Icon = null,
            Moderated = false,
            Name = c.Type == ChannelType.PM ? c.Name : $"#{c.Name}",
            Type = c.Type == ChannelType.PM ? "PM" : "PUBLIC",
            Users = (await _storage.GetChannelAsync(c.Id, new LazerContext())).Users,
            ChannelId = c.Id,
            LastMessageId = null,
            LastReadId = null,
        })));
    }

    [HttpGet("updates")]
    [RequiredLazerClient]
    public async Task<IActionResult> GetUpdates([FromQuery(Name = "since")] long since)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        
        var updates = await _storage.GetUpdatesForUser(user.Id);
        if (since == 0)
            return Json(new Update
            {
                Channels = updates.SelectMany(c => c.Channels ?? new List<Channel>()).ToList(),
                Messages = updates.SelectMany(c => c.Messages)
                    .ToList()
            });

        var channelUpdates = updates.Where(c => !c.IsRead).SelectMany(c => c.Channels ?? new List<Channel>()).ToList();
        var messageUpdates = updates.Where(c => !c.IsRead).SelectMany(c => c.Messages).ToList();
        await _storage.ClearUpdatesForUser(user.Id);
        
        return Json(new Update
        {
            Channels = channelUpdates,
            Messages = messageUpdates
        });
    }

    [HttpPut("channels/{channelid}/users/{userId}")]
    [RequiredLazerClient]
    public async Task<IActionResult> PutUser([FromRoute(Name = "channelid")] int channelId,
        [FromRoute(Name = "userId")] int userId)
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
    public async Task<IActionResult> LeaveUserFromChannel([FromRoute(Name = "channelid")] int channelId,
        [FromRoute(Name = "userId")] int userId)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (channel is null)
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
    public async Task<IActionResult> PostMessageToChannel([FromRoute(Name = "channelid")] int channelId,
        [FromForm] MessagePostBody body)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];

        var channel = await _storage.GetChannelAsync(channelId, _context);

        if (!channel.Users.Contains(user.Id))
            return BadRequest();


        var sender = new Sender
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
        };
        var channelMessage = await _storage.SendMessageToChannel(sender, channel, body.Message, body.IsAction);
        
        
        if (body.Message.StartsWith("!"))
        {
            var command = _commands.GetCommandByName(body.Message);

            if (command is null)
            {
                await _storage.SendMessageToChannel(UserStorage.SystemSender, channel, "Command not found.", false);
                return Json(channelMessage);
            }

            if (command.AdminRequired && !user.IsAdmin)
            {
                await _storage.SendMessageToChannel(UserStorage.SystemSender, channel, "No permissions.", false);
                return Json(channelMessage);
            }

            try
            {
                var arguments = body.Message.Split(" ").ToList().GetRange(1, body.Message.Split(" ").Length - 1);
                var context = new CommandContext(command.Name, arguments.ToArray(), user.Id, channel.ChannelId,
                    _storage);

                var invokeArguments = new List<object> {context};
                invokeArguments.AddRange(arguments.ToArray());

                if (arguments.Count < command.RequiredArguments + 1)
                {
                    for (var i = 0; i < command.RequiredArguments - arguments.Count + 1; i++)
                    {
                        invokeArguments.Add(null);
                    }
                }

                var result = command.Action.Invoke(invokeArguments);

                await _storage.SendMessageToChannel(UserStorage.SystemSender, channel, result, false);
            }
            catch (Exception e)
            {
                await _storage.SendMessageToChannel(UserStorage.SystemSender, channel, e.Message, false);
                return Json(channelMessage);
            }
        }

        return Json(channelMessage);
    }

    [HttpPost("channels")]
    [RequiredLazerClient]
    public async Task<IActionResult> CreateChannelAsync([FromForm] CreatePMForm body)
    {
        var user = _storage.Users[Request.Headers["Authorization"].ToString().Replace("Bearer ", "")];
        var target = _storage.Users.Values.FirstOrDefault(c => c.Id == body.TargetId);

        if (target is null)
            return NotFound();

        var channel = _storage.Channels.Values.FirstOrDefault(c =>
            c.Type == "PM" && c.Users.Contains(body.TargetId) && c.Users.Contains(user.Id));
        if (channel is not null)
            return Json(
                new {channel_id = channel.ChannelId, recent_messages = channel.Messages, name = target.Username});


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

        var channel = _storage.Channels.Values.FirstOrDefault(c =>
            c.Type == "PM" && c.Users.Contains(body.TargetId.Value) && c.Users.Contains(user.Id));

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