using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OsuLazerServer.Database.Tables.Scores;

[Keyless]
public class HitResultStats
{

    
    [JsonPropertyName("None")]
    public int None { get; set; }
    [JsonPropertyName("Miss")]
    public int Misses { get; set; }
    public int Meh { get; set; }
    public int Ok { get; set; }
    [JsonPropertyName("Good")]
    public int Goods { get; set; }
    [JsonPropertyName("Great")]
    public int Greats { get; set; }
    public int Perfect { get; set; }
    [JsonPropertyName("SmallTickMiss")]
    public int SmallTickMisses { get; set; }
    [JsonPropertyName("SmallTickHit")]
    public int SmallTickHits { get; set; }
    [JsonPropertyName("LargeTickMiss")]
    public int LargeTickMisses { get; set; }
    [JsonPropertyName("LargeTickHit")]
    public int LargeTickHits { get; set; }
    public int SmallBonus { get; set; }
    public int LargeBonus { get; set; }
    [JsonPropertyName("IgnoreMiss")]
    public int IgnoreMisses { get; set; }
    [JsonPropertyName("IgnoreHit")]
    public int IgnoreHits { get; set; }
    public Dictionary<string, int> ToOsu()
    {
        /*return new Dictionary<string, int>
        {
            {HitResult.Miss.ToString(), Misses},
            {HitResult.Meh.ToString(), Meh},
            {HitResult.Ok.ToString(), Ok},
            {HitResult.Good.ToString(), Goods},
            {HitResult.Great.ToString(), Greats},
            {HitResult.Perfect.ToString(), Perfect},
            {HitResult.SmallTickMiss.ToString(), SmallTickMisses},
            {HitResult.SmallTickHit.ToString(), SmallTickHits},
            {HitResult.LargeTickHit.ToString(), LargeTickMisses},
            {HitResult.SmallBonus.ToString(), SmallBonus},
            {HitResult.LargeBonus.ToString(), LargeBonus},
            {HitResult.IgnoreMiss.ToString(), IgnoreMisses},
            {HitResult.IgnoreHit.ToString(), IgnoreHits}
        };*/

        return new Dictionary<string, int>
        {
            {"count_100", Ok},
            {"count_300", Greats},
            {"count_50", Meh},
            {"count_geki", LargeBonus},
            {"count_katu", SmallBonus},
            {"count_miss", Misses}
        };
    }
    
    public static HitResultStats FromJson(string statistics) => JsonSerializer.Deserialize<HitResultStats>(statistics);
    public string ToJson() => JsonSerializer.Serialize(this);
}