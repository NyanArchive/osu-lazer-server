using System.Text.Json.Serialization;

namespace OsuLazerServer.Services.Wiki;

public struct WikiInfo
{
    [JsonPropertyName("available_locales")]
    public List<string> AvailableLocales { get; set; }
    [JsonPropertyName("layout")]
    public string Layout { get; set; }
    [JsonPropertyName("locale")]
    public string Locale { get; set; }
    [JsonPropertyName("markdown")]
    public string Content { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
}