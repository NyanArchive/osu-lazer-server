namespace OsuLazerServer.Services.Beatmaps;

public interface IBeatmapSetResolver
{
    public Dictionary<int, object> BeatmapsCache { get; set; }
    public Task<List<BeatmapSet>> FetchSets(string query, int mode, int offset, bool nsfw, string status = "any");

    public Task<BeatmapSet?> FetchSetAsync(int setId);

    public Task<Beatmap?> FetchBeatmap(int beatmapId);
}