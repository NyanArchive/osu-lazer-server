using System.Text;

namespace OsuLazerServer.Services.Users;

public class TokenService : ITokensService
{
    public OAuthToken GenerateToken()
    {
        return new OAuthToken
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", ""),
            ExpiresIn = DateTimeOffset.Now.AddDays(31).ToUnixTimeSeconds()
        };
    }
}