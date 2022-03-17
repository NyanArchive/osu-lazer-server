using System.Text.Json;

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
        return Directory.GetDirectories("data/wiki").Select(Path.GetFileName).Where(wiki => !wiki.StartsWith("_")).ToList();
    }

    public WikiInfo GetWikiPage(string wiki)
    {
        var wikiInfo = JsonSerializer.Deserialize<WikiInfo>(File.ReadAllText(Path.Combine("Data", "wiki", wiki, "info.json")));

        var markdown = File.ReadAllText(Path.Combine("data", "wiki", wiki, "page.md"));

        wikiInfo.Content = markdown;

        return wikiInfo;
    }
}