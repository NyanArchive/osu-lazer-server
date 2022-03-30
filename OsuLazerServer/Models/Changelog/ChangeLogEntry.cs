using System.Text.Json.Serialization;
using osu.Game.Online.API.Requests.Responses;

namespace OsuLazerServer.Models.Changelog;

public class ChangeLogEntry
{
    public class APIChangelogEntry
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("repository")]
        public string Repository { get; set; }

        [JsonPropertyName("github_pull_request_id")]
        public long? GithubPullRequestId { get; set; }

        [JsonPropertyName("github_url")]
        public string GithubUrl { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public ChangelogEntryType Type { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("message_html")]
        public string MessageHtml { get; set; }

        [JsonPropertyName("major")]
        public bool Major { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonPropertyName("github_user")]
        public APIChangelogUser GithubUser { get; set; }
    }

    public enum ChangelogEntryType
    {
        Add,
        Fix,
        Misc
    }
}