using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using MatchType = osu.Game.Online.Rooms.MatchType;

namespace OsuLazerServer.Models.Multiplayer;

public class Room
{
    [JsonProperty("id")]
    public long? RoomID;

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("host")]
    public APIUser Host = new();

    [JsonProperty("playlist")]
    public PlaylistItem Playlist;

    [JsonProperty("channel_id")]
    public int ChannelId;

    [JsonIgnore]
    public RoomCategory Category = new();

    [JsonProperty("category")]
    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    private RoomCategory category
    {
        get => Category;
        set => Category = value;
    }

    [JsonIgnore]
    public int? MaxAttempts;

    [JsonIgnore]
    public RoomStatus Status = new RoomStatusOpen();

    [JsonIgnore]
    public RoomAvailability Availability = new RoomAvailability();

    [JsonIgnore]
    public MatchType Type;

    // Todo: osu-framework bug (https://github.com/ppy/osu-framework/issues/4106)
    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    [JsonProperty("type")]
    private MatchType type
    {
        get => Type;
        set => Type = value;
    }

    [JsonIgnore]
    public QueueMode QueueMode = new();

    [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
    [JsonProperty("queue_mode")]
    private QueueMode queueMode
    {
        get => QueueMode;
        set => QueueMode = value;
    }

    [JsonProperty("current_user_score")]
    public PlaylistAggregateScore UserScore = new();

    [JsonProperty("has_password")]
    public bool HasPassword;

    [JsonProperty("recent_participants")]
    public List<APIUser> RecentParticipants = new();

    [JsonProperty("participant_count")]
    public int ParticipantCount;

    #region Properties only used for room creation request

    [JsonProperty("password")]
    public string Password;

    [JsonIgnore]
    public TimeSpan? Duration;

    [JsonProperty("duration")]
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

    [JsonProperty("ends_at")]
    public DateTimeOffset? EndDate = new DateTimeOffset?();

    // Todo: Find a better way to do this (https://github.com/ppy/osu-framework/issues/1930)
    [JsonProperty("max_attempts", DefaultValueHandling = DefaultValueHandling.Ignore)]
    private int? maxAttempts
    {
        get => MaxAttempts;
        set => MaxAttempts = value;
    }

    #region Newtonsoft.Json implicit ShouldSerialize() methods

    // The properties in this region are used implicitly by Newtonsoft.Json to not serialise certain fields in some cases.
    // They rely on being named exactly the same as the corresponding fields (casing included) and as such should NOT be renamed
    // unless the fields are also renamed.

    [UsedImplicitly]
    public bool ShouldSerializeRoomID() => false;

    [UsedImplicitly]
    public bool ShouldSerializeHost() => false;

    [UsedImplicitly]
    public bool ShouldSerializeEndDate() => false;

    #endregion
}