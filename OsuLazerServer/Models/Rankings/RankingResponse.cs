using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Rankings;

public class RankingResponse
{
    [JsonPropertyName("ranking")]
    public List<RankingUser> Rankings { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}