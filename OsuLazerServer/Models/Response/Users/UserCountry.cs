using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.Users;

public class Country
{
    /// <summary>
    /// The name of this country.
    /// </summary>
    [JsonPropertyName(@"name")]
    public string FullName { get; set; }

    /// <summary>
    /// Two-letter flag acronym (ISO 3166 standard)
    /// </summary>
    [JsonPropertyName(@"code")]
    public string FlagName { get; set; }
}