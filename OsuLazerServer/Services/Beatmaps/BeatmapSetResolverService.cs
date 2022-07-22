using System.Net;
using System.Text.Json;
using BackgroundQueue;
using BackgroundQueue.Generic;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Caching.Memory;

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
        
        using var scope = ServiceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        
        var request = await (new HttpClient()).GetAsync($"https://api.nerinyan.moe/search?m={mode}&p={offset}&s={status}&nsfw={nsfw}&e=&q={query}&sort=ranked_desc&creator=0");

        if (!request.IsSuccessStatusCode)
            return null;

        Console.WriteLine(await request.Content.ReadAsStringAsync());
        var body = JsonSerializer.Deserialize<List<BeatmapSet>>(await request.Content.ReadAsStringAsync());
        
        var background = Scope.ServiceProvider.GetService<IBackgroundTaskQueue>();
        background.Enqueue(async (c) =>
        {
            foreach (var map in body)
            {
                if (cache.Get<BeatmapSet?>(map.Id) is null)
                {
                    cache.Set(map.Id, map, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    });
                }
            }
        });

        return body;
    }

    public async Task<BeatmapSet?> FetchSetAsync(int setId)
    {


        using var scope = ServiceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        
        if (cache.TryGetValue($"beatmapsets_{setId}", out var beatmapsets))
        {
            return (BeatmapSet) beatmapsets!;
        }

        var request = await (new HttpClient()).GetAsync($"https://api.nerinyan.moe/search?q={setId}&option=s");

		Console.WriteLine(await request.Content.ReadAsStringAsync());
        
        var body = JsonSerializer.Deserialize<List<BeatmapSet>>(await request.Content.ReadAsStringAsync()).FirstOrDefault();

      
        if (body is not null)
        {
            foreach (var beatmap in body.Beatmaps)
            {
                cache.Set($"beatmaps_{beatmap.Id}", beatmap);
            }
        }
        cache.Set($"beatmapsets_{setId}", body);
        return body;
    }

    public async Task<Beatmap?> FetchBeatmap(int beatmapId)
    {
        using var scope = ServiceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetService<IMemoryCache>();
        var background = scope.ServiceProvider.GetService<IBackgroundTaskQueue>();
        
        if (cache.TryGetValue($"beatmaps_{beatmapId}", out var beatmap))
        {
            return (Beatmap) beatmap!;
        }

        var request = await (new HttpClient()).GetAsync($"https://api.nerinyan.moe/search?q={beatmapId}&option=m");

        Console.WriteLine(await request.Content.ReadAsStringAsync());
        if (!request.IsSuccessStatusCode)
            return null;
        var body = JsonSerializer.Deserialize<List<Beatmap>>(await request.Content.ReadAsStringAsync()).FirstOrDefault();
        
        
        cache.Set($"beatmaps_{beatmapId}", body);
        return body;
    }
    

    public void Dispose()
    {
        
    }

}