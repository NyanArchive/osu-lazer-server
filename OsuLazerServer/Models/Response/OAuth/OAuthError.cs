using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.OAuth;

public class OAuthError
{
    
    
    [JsonProperty("error")]
    public string ErrorIdentifier { get; set; }

    [JsonProperty("hint")]
    public string Hint { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}