using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Multiplayer;
public class ItemAttemptsCount
{
    [JsonPropertyName("id")]
    public int PlaylistItemID { get; set; }

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }
}

public class PlaylistAggregateScore
{
    [JsonPropertyName("playlist_item_attempts")]
    public ItemAttemptsCount[] PlaylistItemAttempts { get; set; }
}
