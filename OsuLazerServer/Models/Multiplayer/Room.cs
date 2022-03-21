using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using System.Text.Json.Serialization;
using MatchType = osu.Game.Online.Rooms.MatchType;

namespace OsuLazerServer.Models.Multiplayer;

public class Room : ICloneable
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("host")]
    public APIUser Host { get; set; }

    [JsonPropertyName("playlist")]
    public List<PlaylistItem> Playlist { get; set; }

    [JsonPropertyName("channel_id")]
    public int ChannelId { get; set; }

    [JsonPropertyName("current_playlist_item")]
    public PlaylistItem CurrentPlaylistItem { get; set; }

    [JsonPropertyName("playlist_item_stats")]
    public RoomPlaylistItemStats? PlaylistItemStats { get; set; }

    [JsonPropertyName("difficulty_range")]
    public RoomDifficultyRange? DifficultyRange { get; set; }

    [JsonPropertyName("current_user_score")]
    public PlaylistAggregateScore? CurrentUserScore { get; set; }

    [JsonPropertyName("has_password")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("recent_participants")]
    public List<APIUser> RecentParticipants { get; set; }

    [JsonPropertyName("participant_count")]
    public int? ParticipantCount { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("queue_mode")]
    public string QueueMode { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }
    
    [JsonPropertyName("max_attemps")]
    public int MaxAttempts { get; set; }

    public object Clone()
    {
        return this;
    }
}

public class RoomDifficultyRange
{
    [JsonPropertyName("min")]
    public double Min { get; set; }

    [JsonPropertyName("max")]
    public double Max { get; set; }
}

public class RoomPlaylistItemStats
{
    [JsonPropertyName("count_active")]
    public int CountActive { get; set; }

    [JsonPropertyName("count_total")]
    public int CountTotal { get; set; }

    [JsonPropertyName("ruleset_ids")]
    public int[] RulesetIDs { get; set; }
}
