using System.Net;
using System.Text.Json;
using BackgroundQueue;
using BackgroundQueue.Generic;

namespace OsuLazerServer.Services.Beatmaps;

public class BeatmapSetResolverService : IBeatmapSetResolver, IServiceScope
{
    public Dictionary<int, object> BeatmapsCache { get; set; } = new();

    
    public IServiceProvider ServiceProvider { get; }
    public IServiceScope Scope { get; set; }
    public BeatmapSetResolverService(IServiceProvider services)
    {
        ServiceProvider = services;
        Scope = ServiceProvider.CreateScope();
    }
    public async Task<List<BeatmapSet?>> FetchSets(string query, int mode, int offset, bool nsfw, string status = "any")
    {
        var request = await (new HttpClient()).GetAsync($"https://rus.nerinyan.moe/search?m={mode}&p={offset}&s={status}&nsfw={nsfw}&e=&q={query}&sort=ranked_desc&creator=0&ps=300");

        if (!request.IsSuccessStatusCode)
            return null;
        var body = JsonSerializer.Deserialize<List<BeatmapSet>>(await request.Content.ReadAsStringAsync());


        foreach (var map in body)
        {
            if (!BeatmapsCache.ContainsKey(map.Id))
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
        
        var request = await (new HttpClient()).GetAsync($"https://rus.nerinyan.moe/search/beatmapset/{setId}");

        var body = JsonSerializer.Deserialize<BeatmapSet>(await request.Content.ReadAsStringAsync());
        if (body is not null)
        {
            BeatmapsCache.TryAdd(setId, body);
            foreach (var beatmap in body.Beatmaps)
            {
                BeatmapsCache.TryAdd(beatmap.Id, beatmap);
            }
        }
        return body;
    }

    public async Task<Beatmap> FetchBeatmap(int beatmapId)
    {
        if (BeatmapsCache.TryGetValue(beatmapId, out var beatmap))
        {
            if (beatmap is Beatmap value)
                return value;
        }

        var request = await (new HttpClient()).GetAsync($"https://rus.nerinyan.moe/search/beatmap/{beatmapId}");

        var body = JsonSerializer.Deserialize<Beatmap>(await request.Content.ReadAsStringAsync());
        
        if (body is not null)
        {
            var cache = Scope.ServiceProvider.GetService<IBackgroundTaskQueue>();
            cache.Enqueue(async (c) =>
            {
                await FetchSetAsync(body.BeatmapsetId);
            });

        }
        return body;
    }
    

    public void Dispose()
    {
        
    }

}