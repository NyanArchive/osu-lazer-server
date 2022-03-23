using System.Text.Json;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Services.Beatmaps;

namespace OsuLazerServer.Utils;


/**
 * public enum BeatmapOnlineStatus
{
    None = -3,
    Graveyard = -2,
    WIP = -1,
    Pending = 0,
    Ranked = 1,
    Approved = 2,
    Qualified = 3,
    Loved = 4,
}
 */
public class BeatmapUtils
{

    public static Dictionary<int, Stream> BeatmapStreamCache { get; set; } = new();
    public static BeatmapOnlineStatus Status(string status)
    {
        var result = BeatmapOnlineStatus.None;
        switch (status)
        {
            case "ranked":
                result = BeatmapOnlineStatus.Ranked;
                break;
            case "loved":
                result = BeatmapOnlineStatus.Loved;
                break;
            case "graveyard":
                result = BeatmapOnlineStatus.Graveyard;
                break;
            case "approved":
                result = BeatmapOnlineStatus.Approved;
                break;
            case "pending":
                result = BeatmapOnlineStatus.Pending;
                break;
            case "qualified":
                result = BeatmapOnlineStatus.Qualified;
                break;
            case "wip":
                result = BeatmapOnlineStatus.WIP;
                break;
        }

        return result;
    }

    public static async Task<Stream> GetBeatmapStream(int beatmapId)
    {
        /*if (BeatmapStreamCache.TryGetValue(beatmapId, out var stream))
            return stream;*/
        var response = await (await new HttpClient().GetAsync($"https://osu.ppy.sh/osu/{beatmapId}")).Content.ReadAsStreamAsync();
        response.Seek(0, SeekOrigin.Begin);
        
        /*
        BeatmapStreamCache.Add(beatmapId, response);*/
        return response;
    }

    public static async Task<string> GetBeatmapStatus(int beatmapId)
    {
        var request = await (new HttpClient()).GetAsync($"https://rus.nerinyan.moe/search/beatmap/{beatmapId}");

        var body = JsonSerializer.Deserialize<Beatmap>(await request.Content.ReadAsStringAsync());
        
        return body?.Status??"graveyard";
    }


    public static ScoreRank ScoreRankFromString(string rank) => rank switch
    {
        "A" => ScoreRank.A,
        "B" => ScoreRank.B,
        "C" => ScoreRank.C,
        "D" => ScoreRank.D,
        "S" => ScoreRank.S,
        "X" => ScoreRank.X,
        "SH" => ScoreRank.SH,
        "XH" => ScoreRank.XH,
        _ => ScoreRank.D
    };
}