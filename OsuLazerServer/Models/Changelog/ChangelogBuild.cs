using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Changelog;

public class ChangelogBuild
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("display_version")]
    public string DisplayVersion { get; set; }

    [JsonPropertyName("users")]
    public long Users { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("update_stream")]
    public UpdateStream UpdateStream { get; set; }

    [JsonPropertyName("changelog_entries")]
    public List<ChangeLogEntry> ChangelogEntries { get; set; }

    [JsonPropertyName("versions")]
    public VersionNavigation Versions { get; set; }

    public string Url => $"https://osu.ppy.sh/home/changelog/{UpdateStream?.Name??"Unknown"}/{Version}";

    public class VersionNavigation
    {
        [JsonPropertyName("next")]
        public ChangelogBuild Next { get; set; }

        [JsonPropertyName("previous")]
        public ChangelogBuild Previous { get; set; }
    }

    public ChangelogBuild WithoutStream()
    {
        UpdateStream = null;
        return this;
    }
}