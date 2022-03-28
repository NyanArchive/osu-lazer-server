using System.Text.Json.Serialization;

namespace OsuLazerServer.Services.Wiki.News;

public struct NewsEntry
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("edit_url")]
    public string EditUrl { get; set; }

    [JsonPropertyName("first_image")]
    public string FirstImage { get; set; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("preview")]
    public string Preview { get; set; }
}