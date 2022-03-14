using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.Users;

public class APIRankHistory
{
    [JsonProperty(@"mode")]
    public string Mode;

    [JsonProperty(@"data")]
    public int[] Data;
}