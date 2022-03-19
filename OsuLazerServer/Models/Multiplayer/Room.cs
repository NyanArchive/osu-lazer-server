using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using System.Text.Json.Serialization;
using MatchType = osu.Game.Online.Rooms.MatchType;

namespace OsuLazerServer.Models.Multiplayer;

public class Room
{
    [JsonPropertyName("id")]
    public long? RoomID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("host")]
    public APIUser Host { get; set; }

    [JsonPropertyName("playlist")]
    public List<PlaylistItem> Playlist { get; set; }

    [JsonPropertyName("current_playlist_item")]
    public PlaylistItem CurrentPlaylistItem { get; set; }

    [JsonPropertyName("channel_id")]
    public int ChannelId { get; set; }

    public RoomCategory Category { get; set; }

    [JsonPropertyName("category")]
    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    private RoomCategory category
    {
        get => Category;
        set => Category = value;
    }

    public int? MaxAttempts;

    [JsonIgnore]
    public MatchType Type;

    // Todo: osu-framework bug (https://github.com/ppy/osu-framework/issues/4106)
    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    [JsonPropertyName("type")]
    private MatchType type
    {
        get => Type;
        set => Type = value;
    }

    [JsonIgnore]
    public QueueMode QueueMode { get; set; }

    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    [JsonPropertyName("queue_mode")]
    private QueueMode queueMode
    {
        get => QueueMode;
        set => QueueMode = value;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("current_user_score")]
    public PlaylistAggregateScore UserScore { get; set; }

    [JsonPropertyName("has_password")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("recent_participants")]
    public List<APIUser> RecentParticipants { get; set; }

    [JsonPropertyName("participant_count")]
    public int ParticipantCount { get; set; }

    #region Properties only used for room creation request

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonIgnore]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("duration")]
    private int? duration
    {
        get => (int?)Duration?.TotalMinutes;
        set
        {
            if (value == null)
                Duration = null;
            else
                Duration = TimeSpan.FromMinutes(value.Value);
        }
    }

    #endregion

    // Only supports retrieval for now

    [JsonPropertyName("ends_at")]
    public DateTimeOffset? EndDate = new DateTimeOffset?();

    // Todo: Find a better way to do this (https://github.com/ppy/osu-framework/issues/1930)
    [JsonPropertyName("max_attempts")]
    private int? maxAttempts
    {
        get => MaxAttempts;
        set => MaxAttempts = value;
    }
}