using osu.Game.Online.API;
using OsuLazerServer.Models.Response.Beatmaps;
using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Multiplayer;

public class PlaylistItem
{
    [JsonPropertyName("id")]
    public long ID { get; set; }

    [JsonPropertyName("owner_id")]
    public int OwnerID { get; set; }

    [JsonPropertyName("ruleset_id")]
    public int RulesetID { get; set; }

    /// <summary>
    /// Whether this <see cref="PlaylistItem"/> is still a valid selection for the <see cref="Room"/>.
    /// </summary>
    [JsonPropertyName("expired")]
    public bool Expired { get; set; }

    [JsonPropertyName("playlist_order")]
    public ushort? PlaylistOrder { get; set; }

    [JsonPropertyName("played_at")]
    public DateTimeOffset? PlayedAt { get; set; }

    [JsonPropertyName("allowed_mods")]
    public APIMod[] AllowedMods { get; set; } = Array.Empty<APIMod>();

    [JsonPropertyName("required_mods")]
    public APIMod[] RequiredMods { get; set; } = Array.Empty<APIMod>();

    /// <summary>
    /// Used for deserialising from the API.
    /// </summary>
    [JsonPropertyName("beatmap")]
    public APIBeatmap ApiBeatmap { get; set; }

    /// <summary>
    /// Used for serialising to the API.
    /// </summary>
    [JsonPropertyName("beatmap_id")]
    public int OnlineBeatmapId { get; set; }

    /// <summary>
    /// A beatmap representing this playlist item.
    /// In many cases, this will *not* contain any usable information apart from OnlineID.
    /// </summary>
    [JsonIgnore]
    public APIBeatmap Beatmap { get; set; }

    private bool valid = true;

    [JsonConstructor]
    private PlaylistItem()
    {
    }

    public PlaylistItem(APIBeatmap beatmap)
    {
        Beatmap = beatmap;
    }

    public void MarkInvalid() => valid = false;

    public PlaylistItem With(APIBeatmap beatmap) => new (beatmap)
    {
        ID = ID,
        OwnerID = OwnerID,
        RulesetID = RulesetID,
        Expired = Expired,
        PlaylistOrder = PlaylistOrder,
        PlayedAt = PlayedAt,
        AllowedMods = AllowedMods,
        RequiredMods = RequiredMods,
        valid = valid,
    };
}