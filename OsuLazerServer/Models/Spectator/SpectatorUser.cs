using System.Text.Json.Serialization;
using OsuLazerServer.Database.Tables;
using Realms;

namespace OsuLazerServer.Models.Spectator;

public class SpectatorUser : APIUser
{
    
    [JsonPropertyName("statistics_rulesets")]
    public Dictionary<string, Statistics> StatisticsRuleset { get; set; }
}