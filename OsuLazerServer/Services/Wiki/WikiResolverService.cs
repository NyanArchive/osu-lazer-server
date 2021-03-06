using System.Text.Json;
using OsuLazerServer.Services.Wiki.News;

namespace OsuLazerServer.Services.Wiki;

public class WikiResolverService : IWikiResolver
{
    public Dictionary<string, WikiInfo> WikiCache { get; set; } = new();
    
    public WikiInfo GetWikiByPage(string titlePath)
    {
        if (WikiCache.TryGetValue(titlePath, out var cachedWiki))
            return cachedWiki;

        var wikis = ListOfWikis();

        if (!wikis.Contains(titlePath))
        {
            return new WikiInfo
            {
                Content = "# Not found",
                Layout = "not_found",
                Locale = "en",
                Path = "_NotFound",
                Subtitle = null,
                Tags = new List<string>(),
                Title = "Page not found",
                AvailableLocales = new List<string> { "en" }
            };
        }


        var wiki = GetWikiPage(titlePath);

        return wiki;
    }

    public List<string> ListOfWikis()
    {
        return Directory.GetDirectories(Path.Join("data", "wiki")).Select(Path.GetFileName).Where(wiki => !wiki.StartsWith("_")).ToList();
    }

    public WikiInfo GetWikiPage(string wiki)
    {
        var wikiInfo = JsonSerializer.Deserialize<WikiInfo>(File.ReadAllText(Path.Combine("data", "wiki", Path.GetFileName(wiki), "info.json")));

        var markdown = File.ReadAllText(Path.Combine("data", "wiki", Path.GetFileName(wiki), "page.md"));

        wikiInfo.Content = markdown;

        return wikiInfo;
    }

    public List<string> ListOfNews()
    {
        return Directory.GetDirectories(Path.Join("data", "news")).Select(Path.GetFileName).ToList();
    }

    public NewsEntry GetNewsEntry(string news)
    {
        var result = JsonSerializer.Deserialize<NewsEntry>(File.ReadAllText(Path.Combine("data", "news", Path.GetFileName(news), "data.json")));
        return result;
    }
}