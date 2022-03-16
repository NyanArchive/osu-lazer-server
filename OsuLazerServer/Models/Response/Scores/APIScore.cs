using System.Text.Json.Serialization;
using Newtonsoft.Json;
using osu.Game.Online.API;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models.Response.Beatmaps;

namespace OsuLazerServer.Models.Response.Scores;


public class APIScore
{
    [JsonPropertyName("score")]
    public long TotalScore { get; set; }
    
    
    [JsonPropertyName(@"max_combo")]
    public int MaxCombo { get; set; }
    
    [JsonPropertyName(@"user")]
    public APIUser User { get; set; }
    
    [JsonPropertyName(@"id")]   
    public long OnlineID { get; set; }
        
    [JsonPropertyName(@"replay")]
    public bool HasReplay { get; set; }
    
    
    [JsonPropertyName(@"created_at")]
    public DateTimeOffset Date { get; set; }
    
    [JsonPropertyName(@"beatmap")]
    public APIBeatmap? Beatmap { get; set; }
    
    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }
    
    [JsonPropertyName(@"pp")]
    public double? PP { get; set; }
    
    [JsonPropertyName(@"beatmapset")]
    public object? beatmapSet { get; set; }
    
    [JsonPropertyName("statistics")]
    public object Statistics { get; set; }
    
    [JsonPropertyName(@"mode_int")]
    public int RulesetID { get; set; }
    
    [JsonPropertyName(@"mods")]        
    public object Mods { get;set; }
    
    [JsonPropertyName("rank")]
    public string Rank { get; set; }
}