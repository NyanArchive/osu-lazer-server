using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using OsuLazerServer.Database;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Models.Multiplayer;
using OsuLazerServer.SpectatorClient;
using UniqueIdGenerator.Net;
using Channel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Services.Users;

public class UserStorage : IUserStorage, IServiceScope
{
    public Dictionary<string, User> Users { get; set; } = new ();
    public Dictionary<long, User> ScoreTokens { get; set; } = new();
    public Dictionary<int, List<DbScore>> LeaderboardCache { get; set; } = new();
    public Dictionary<string, List<User>> GlobalLeaderboardCache { get; set; } = new();
    public Dictionary<int, Room> Rooms { get; set; } = new();
    public Dictionary<int, Update> Updates { get; set; } = new();
    public Dictionary<int, Channel> Channels { get; set; } = new();
    public Dictionary<int, SpectatorState> UserStates { get; set; } = new();
    public Dictionary<int, MultiplayerRoom> HubRooms { get; set; } = new();
    public Dictionary<int, PlaylistItem> PlaylistItems { get; set; } = new();
    public IServiceScope Scope { get; set; }
    public IServiceProvider ServiceProvider { get; }
    public UserStorage(IServiceProvider scope)
    {
        ServiceProvider = scope;
        Scope = scope.CreateScope();
    }
    public static Sender SystemSender { get; set; } = new Sender
    {
        Id = 1,
        Username = "Oleg",
        AvatarUrl = "https://media.discordapp.net/attachments/944308912671322122/952827877290807316/file_198.jpg",
        CountryCode = "UA",
        DefaultGroup = "bot",
        IsActive = true,
        IsBot = true,
        IsDeleted = false,
        IsOnline = true,
        IsSupporter = true,
        LastVisit = DateTime.Now,
        ProfileColour = "#FFFFFF",
        PmFriendsOnly = false
    };
    public async Task AddUpdate(int userId, Update update)
    {
        if (!Updates.TryGetValue(userId, out var updates))
        {
            var newUpdate = new Update
            {
                Channels = new List<Channel>(),
                Messages = new List<Message>(),
                UpdateRecievedAt = DateTimeOffset.Now
            };
            if (updates is null)
                return;
            updates.Channels?.AddRange(update.Channels ?? new List<Channel>());
            newUpdate.Messages.AddRange(update.Messages);
            Updates.Add(userId, newUpdate);
            return;
        }
        
        updates.Channels?.AddRange(update.Channels ?? new List<Channel>());
        updates.Messages.AddRange(update.Messages);
        updates.UpdateRecievedAt = DateTimeOffset.Now;
    }

    public async Task<Update> GetUpdatesForUser(int userId)
    {
        if (!Updates.TryGetValue(userId, out var updates))
        {
            var update = new Update
            {
                Channels = new List<Channel>(),
                Messages = new List<Message>(),
                UpdateRecievedAt = DateTimeOffset.Now
            };
            Updates.TryAdd(userId, update);
            return update;
        }
        
        return updates;
    }

    public async Task<Channel> GetChannelAsync(int channelId, LazerContext context, bool forceFetch = false)
    {
        if (Channels.TryGetValue(channelId, out var channel))
            return channel;

        var dbChannel = await context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);

        if (dbChannel is null)
        {
            return new Channel();
        }
        var newChannel = new Channel
        {
            Description = dbChannel.Description,
            Icon = null,
            Messages = new List<Message>
            {
                new Message
                {
                    Content = $"Welcome to #{dbChannel.Name}",
                    Sender = SystemSender,
                    Timetamp = DateTime.Now,
                    ChannelId = dbChannel.Id,
                    MessageId = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    SenderId = SystemSender.Id
                }
            },
            Moderated = false,
            Name = dbChannel.Name,
            Type = dbChannel.Type == ChannelType.PM ? "PM" : "PUBLIC",
            Users = new List<int>(),
            ChannelId = dbChannel.Id,
            LastMessageId = null,
            LastReadId = null
        };
        
        Channels.Add(newChannel.ChannelId, newChannel);
        
        return newChannel;
    }
    public async Task ForceJoinChannel(int userId, int channelId)
    {
        var channel = await GetChannelAsync(channelId, new LazerContext());

        await AddUpdate(userId, new Update
        {
            Channels = new List<Channel> { channel },
            Messages = new List<Message>()
        });
    }

    public User? GetUser(string token)
    {
        if (!Users.TryGetValue(token, out var u))
        {
            var cache = ServiceProvider.GetRequiredService<IDistributedCache>();
            var cachedUserId = cache.Get($"token:{token}");
            //We trying to get it from redis.
            if (cachedUserId is not null)
            {
                var expiresAtRaw =  Convert.ToInt64(Encoding.UTF8.GetString(cache.Get($"token:{token}:expires_at") ?? Array.Empty<byte>()));

                if (expiresAtRaw <
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return null;
                }

                var user = new LazerContext().Users.First(u => u.Id == Convert.ToInt32(Encoding.UTF8.GetString(cachedUserId)));

                Users.TryAdd(token, user);
                return null;
            }
        }

        return u;
    }

    public async Task NotifyUser(int userId, string message)
    {
        await AddUpdate(userId, new Update
        {
            Channels = new List<Channel>
            {
                new Channel
                {
                    Description = "System messages for user.",
                    Icon = null,
                    Moderated = false,
                    Name = "System",
                    Type = "PM",
                    Users = new List<int> {userId, 1},
                    ChannelId = 1,
                    LastMessageId = 1,
                    LastReadId = null
                }
            },
            Messages = new List<Message>
            {
                new Message
                {
                    Content = message,
                    Sender = SystemSender,
                    Timetamp = DateTime.Now,
                    ChannelId = 1,
                    MessageId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 1000,
                    SenderId = 1
                }
            }
        });
    }
    public void Dispose()
    {
        Scope.Dispose();
    }
}