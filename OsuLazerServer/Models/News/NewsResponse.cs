using System.Text.Json.Serialization;
using osu.Game.Online.API.Requests.Responses;
using OsuLazerServer.Services.Wiki.News;

namespace OsuLazerServer.Models.News;

public class NewsResponse
{
    [JsonPropertyName("news_posts")]
    public IEnumerable<NewsEntry> NewsPosts { get; set; }

    [JsonPropertyName("news_sidebar")]
    public NewsSidebar SidebarMetadata { get; set; }
}