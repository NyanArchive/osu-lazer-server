using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Rankings;
using Response.Users;

public class RankingCountry
{
    [JsonPropertyName("active_users")]
    public int ActiveUsers { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("country")]
    public Country Country { get; set; }
    [JsonPropertyName("performance")]
    public int Performance { get; set; }
    [JsonPropertyName("play_count")]
    public int PlayCount { get; set; }
    [JsonPropertyName("ranked_score")]
    public int RankedScore { get; set; }
}