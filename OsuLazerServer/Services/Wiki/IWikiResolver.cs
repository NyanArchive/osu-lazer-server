using OsuLazerServer.Services.Wiki.News;

namespace OsuLazerServer.Services.Wiki;

public interface IWikiResolver
{
    public Dictionary<string, WikiInfo> WikiCache { get; set; }


    public WikiInfo GetWikiByPage(string titlePath);


    public List<string> ListOfWikis();

    public WikiInfo GetWikiPage(string wiki);

    public List<string> ListOfNews();
    
    public NewsEntry GetNewsEntry(string news);


}