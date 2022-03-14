using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.Users;


public class RegistrationError
{
    [JsonPropertyName("form_error")]
    public RegistrationRequestErrors FormError { get; set; }
}
public class RegistrationRequestErrors
{
    [JsonProperty("user")]
    public UserErrors User { get; set; }

    public class UserErrors
    {
        [JsonProperty("username")]
        public string[] Username { get; set; }

        [JsonProperty("user_email")]
        public string[] Email { get; set; }

        [JsonProperty("password")]
        public string[] Password { get; set; }
    }
}