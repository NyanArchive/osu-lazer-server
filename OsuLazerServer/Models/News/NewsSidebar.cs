using System.Text.Json.Serialization;
using OsuLazerServer.Services.Wiki.News;

namespace OsuLazerServer.Models.News;

public class NewsSidebar
{
    [JsonPropertyName("current_year")]
    public int CurrentYear { get; set; }
    
    [JsonPropertyName("news_posts")]
    public IEnumerable<NewsEntry> NewsPosts { get; set; }
    
    [JsonPropertyName("years")]

    public int[] Years { get; set; }
}