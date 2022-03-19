using System.Net;
using System.Text.Json;

namespace OsuLazerServer.Services.Beatmaps;

public class BeatmapSetResolverService : IBeatmapSetResolver
{
    public Dictionary<int, object> BeatmapsCache { get; set; } = new();

    public async Task<List<BeatmapSet?>> FetchSets(string query, string mode, int offset, bool nsfw, string status = "any")
    {
        var request = await (new HttpClient()).GetAsync($"https://api.nerina.pw/search?m={mode}&p={offset}&s={status}&nsfw={nsfw}&e=&q={query}&sort=ranked_desc&creator=0");

        if (!request.IsSuccessStatusCode)
            return null;
        var body = JsonSerializer.Deserialize<List<BeatmapSet>>(await request.Content.ReadAsStringAsync());


        foreach (var map in body)
        {
            BeatmapsCache.Add(map.Id, map);
        }
        
        return body;
    }

    public async Task<BeatmapSet?> FetchSetAsync(int setId)
    {
        if (BeatmapsCache.TryGetValue(setId, out var set))
        {
            if (set is BeatmapSet value)
                return value;
        } 
        
        var request = await (new HttpClient()).GetAsync($"https://api.nerina.pw/search/beatmapset/{setId}");

        var body = JsonSerializer.Deserialize<BeatmapSet>(await request.Content.ReadAsStringAsync());
        if (body is not null)
        {
            BeatmapsCache.TryAdd(setId, body);
        }
        return body;
    }

    public async Task<Beatmap> FetchBeatmap(int beatmapId)
    {
        if (BeatmapsCache.TryGetValue(beatmapId, out var set))
        {
            if (set is Beatmap value)
                return value;
        }

        var request = await (new HttpClient()).GetAsync($"https://api.nerina.pw/search/beatmap/{beatmapId}");

        var body = JsonSerializer.Deserialize<Beatmap>(await request.Content.ReadAsStringAsync());
        
        if (body is not null)
        {
            BeatmapsCache.TryAdd(beatmapId, body);
        }
        return body;
    }
}