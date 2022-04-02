using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace OsuLazerServer.Database.Tables;

[Table("ruleset_stats")]
public class RuleSetStats
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Required]
    public int Id { get; set; }
    [Column("user_id")]
    public int UserId { get; set; }
    [Column("ruleset_id")]
    public int RulesetId { get; set; }
    [Column("ruleset_stats_json")]
    public string RulesetStatsRaw { get; set; }
    [Column("ruleset_name")]
    public string RulesetName { get; set; }
    
    
    public IUserStats GetRulesetStats()
    {
        return JsonConvert.DeserializeObject<IUserStats>(RulesetStatsRaw);
    }
    
    public void SetUserStats(IUserStats stats)
    {
        RulesetStatsRaw = JsonConvert.SerializeObject(stats);
    }
}