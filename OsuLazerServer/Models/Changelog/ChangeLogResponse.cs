using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Changelog;

public class ChangeLogResponse
{
    [JsonPropertyName("builds")]
    public List<ChangelogBuild> Builds { get; set; }

    [JsonPropertyName("streams")]
    public List<UpdateStream> Streams { get; set; }
}