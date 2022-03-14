using System.Text.Json.Serialization;
using Newtonsoft.Json;
using OsuLazerServer.Models;

namespace OsuLazerServer.Services.Users;


public class OAuthToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    [JsonPropertyName("expires_in")] public long ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
}
public interface ITokensService
{
    public OAuthToken GenerateToken(); //User will be added later
}