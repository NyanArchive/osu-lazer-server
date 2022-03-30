using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Changelog;

public class UpdateStream
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("is_featured")]
    public bool IsFeatured { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("latest_build")]
    public ChangelogBuild LatestBuild { get; set; }

    public UpdateStream WithoutBuild()
    {
        return new UpdateStream
        {
            Id = Id,
            Name = Name,
            DisplayName = DisplayName,
            IsFeatured = IsFeatured,
            LatestBuild = null
        };
    }
}