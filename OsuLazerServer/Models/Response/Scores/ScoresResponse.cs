using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Response.Scores;

public class UserScore
{
    [JsonPropertyName("position")] public int? Position { get; set; }
    
    [JsonPropertyName("score")] public APIScore? Score { get; set; }
}

public class ScoresResponse
{
    [JsonPropertyName("scores")] public List<APIScore> Scores { get; set; }

    [JsonPropertyName("userScore")] public UserScore? UserScore;
}