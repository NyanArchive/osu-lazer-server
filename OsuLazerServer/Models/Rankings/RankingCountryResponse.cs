using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Rankings;

public class RankingCountryResponse
{
    [JsonPropertyName("ranking")]
    public List<RankingCountry> Rankings { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}