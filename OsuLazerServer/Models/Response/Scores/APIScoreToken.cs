using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Response.Scores;

public class APIScoreToken
{
    
    [JsonPropertyName("beatmap_id")]
    public int Beatmap { get; set; }
    
    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("user_id")] public int UserId { get; set; }
    [JsonPropertyName("id")]
    public long Id { get; set; }
}