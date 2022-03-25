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
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.SpectatorClient;
using OsuLazerServer.Utils;
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
    public Dictionary<int, List<Update>> Updates { get; set; } = new();
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
            Updates.Add(userId, new List<Update> { update });
            return;
        }
        update.UpdateRecievedAt = DateTimeOffset.Now;
        updates.Add(update);
    }
    public async Task<List<Update>> GetUpdatesForUser(int userId)
    {
        if (!Updates.TryGetValue(userId, out var updates))
        {
            Updates.Add(userId, new List<Update>());
            return Updates[userId];
        }
        
        return updates;
    }
    public async Task<Channel?> GetChannelAsync(int channelId, LazerContext context, bool forceFetch = false)
    {
        if (Channels.TryGetValue(channelId, out var channel))
            return channel;

        var dbChannel = await context.Channels.FirstOrDefaultAsync(c => c.Id == channelId);

        if (dbChannel is null)
            return null;
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
    
    public async Task<int> GetUserRank(int userId, int ruleSetId, bool forceFetch = false)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;

        var rank = 0;
        if (!forceFetch)
        {
            var cachedRank = await GetCachedInt($"leaderboard:{ruleSetId}:{userId}:rank");
            if (cachedRank is not null)
            {
                return cachedRank.Value;
            }
        }

        var context = new LazerContext();

        Dictionary<int, IUserStats> stats = await GetLeaderboard(ruleSetId);


        foreach (var kvp in stats)
        {
            if (await GetUserPerformancePoints(kvp.Value.Id, ruleSetId) == 0)
                continue;
            if (context.Users.FirstOrDefault(c => c.Id == kvp.Value.Id)?.Banned??false)
                continue;
            await cache.SetAsync($"leaderboard:{ruleSetId}:{userId}:rank", BitConverter.GetBytes(kvp.Key));
        }
        
        return stats.FirstOrDefault(c => c.Value.Id == userId).Key;
    }

    public async Task<double> GetUserPerformancePoints(int userId, int mode, bool forceFetch = false)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;

        if (!forceFetch)
        {
            var cachedRank = await cache.GetAsync($"leaderboard:{mode}:{userId}:performance");
            
            if (cachedRank is not null)
            {
                return BitConverter.ToDouble(cachedRank);
            }
        }

        var context = new LazerContext();

        var userScores = context.Scores.Where(c => c.RuleSetId == mode && c.UserId == userId && c.Passed && c.Status == DbScoreStatus.BEST && !c.Mods.Contains("RX"));

        double performance = 0;

        foreach (var score in userScores)
        {
            var status = await BeatmapUtils.GetBeatmapStatus(score.BeatmapId);
            Console.WriteLine($"beatmap {score.BeatmapId}: status => {status}");
            if (status != "ranked")
                continue;
            performance += score.PerfomancePoints;
        }

        await cache.SetAsync($"leaderboard:{mode}:{userId}:performance", BitConverter.GetBytes(performance));
        return performance;
    }

    public async Task<double> GetUserHitAccuracy(int userId, int mode, bool forceFetch = false)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;

        var rank = 0;
        if (!forceFetch)
        {
            var cachedRank = await cache.GetAsync($"leaderboard:{mode}:{userId}:hitaccuracy");

            if (cachedRank is not null)
            {
                return BitConverter.ToDouble(cachedRank);
            }
        }
        
        var context = new LazerContext();

        var userScores = context.Scores.Where(c => c.UserId == userId && c.Passed && c.Status == DbScoreStatus.BEST);
        
        Console.WriteLine($"User scores: {userId}: {userScores.Count()} => {(userScores.Any() ? userScores.Select(c => c.Accuracy).ToList().Average() : 0)}");
        if (await userScores.CountAsync() == 0)
            return 0.0;
        var accuracy = userScores.Select(c => c.Accuracy).ToList().Average();

        await cache.SetAsync($"leaderboard:{mode}:{userId}:hitaccuracy", BitConverter.GetBytes(accuracy));
        return accuracy;
    }

    public async Task<double> UpdateRankings(string mode)
    {
        
        Console.WriteLine("Calculating new rankings...");
        var cache = ServiceProvider.GetService<IDistributedCache>()!;

        var context = new LazerContext();

        var rulesetId = mode switch
        {
            "osu" => 0,
            "taiko" => 1,
            "fruits" => 2,
            "mania" => 3,
            _ => 0
        };
        var leaderboard = await GetLeaderboard(rulesetId);
        for (var position = 0; position < leaderboard.Count; position++)
        {
            var user = context.Users.FirstOrDefault(c => c.Id == leaderboard.ToList()[position].Value.Id);


            var stats = user.FetchStats(mode);
            
            var perfomance = await GetUserPerformancePoints(user.Id, rulesetId);
       

            var rank = await GetUserRank(user.Id, rulesetId);
            
            Console.WriteLine($"RANK {user.Id} - DB: {leaderboard.FirstOrDefault(c => c.Value.Id == user.Id).Key} - CALC: {rank} - POSITION " + position);
            if (rank != leaderboard.FirstOrDefault(c => c.Value.Id == user.Id).Key)
            {
                await cache.SetAsync($"leaderboard:{rulesetId}:{user.Id}:rank", BitConverter.GetBytes(leaderboard.FirstOrDefault(c => c.Value.Id == user.Id).Key));
                
                
                Console.WriteLine($"Ranking updated. {user.Username} => {await GetUserRank(user.Id, rulesetId)}");
            }
        }
        
        GlobalLeaderboardCache.Clear();
        
        return 0;
    }

    public async Task<double> UpdatePerformance(string mode, int userId, double peromance)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;
        var context = new LazerContext();
        var user = context.Users.FirstOrDefault(c => c.Id == userId);

        var stats = user.FetchStats(mode);

        var currentAccuracy = await GetUserPerformancePoints(userId, mode switch
        {
            "osu" => 0,
            "taiko" => 1,
            "fruits" => 2,
            "mania" => 3,
            _ => 0
        }) + peromance;
        await cache.SetAsync($"leaderboard:{mode}:{userId}:performance", BitConverter.GetBytes(currentAccuracy));
        stats.PerformancePoints = (int)peromance;

        await context.SaveChangesAsync();
        return currentAccuracy;
    }
    
    public async Task<double> UpdateHitAccuracy(string mode, int userId, double accuracy)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;
        var context = new LazerContext();
        var user = context.Users.FirstOrDefault(c => c.Id == userId);

        var stats = user.FetchStats(mode);

        var currentAccuracy = (await GetUserHitAccuracy(userId, mode switch
        {
            "osu" => 0,
            "taiko" => 1,
            "fruits" => 2,
            "mania" => 3,
            _ => 0
        }) + accuracy) / 2;
        await cache.SetAsync($"leaderboard:{mode}:{userId}:hitaccuracy", BitConverter.GetBytes(currentAccuracy));
        stats.Accuracy = (float)currentAccuracy;
        return currentAccuracy;
    }


    public async Task<Dictionary<int, IUserStats>> GetLeaderboard(int ruleset)
    {
        var leaderboardUtils = new LeaderboardUtils(this);
        return ruleset switch
        {
            0 => await leaderboardUtils.GetLeaderboardForOsu(),
            1 => await leaderboardUtils.GetLeaderboardForTaiko(),
            2 => await leaderboardUtils.GetLeaderboardForFruits(),
            3 => await leaderboardUtils.GetLeaderboardForMania()
        };
    }

    
    private async Task<int?> GetCachedInt(string key)
    {
        var cache = ServiceProvider.GetService<IDistributedCache>()!;

        var rawValue = await cache.GetAsync(key);

        if (rawValue is null)
            return null;

        return BitConverter.ToInt32(rawValue);
    }
    public void Dispose()
    {
        Scope.Dispose();
    }
}