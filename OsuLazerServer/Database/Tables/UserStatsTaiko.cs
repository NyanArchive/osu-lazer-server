using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OsuLazerServer.Database.Tables;


[Table("stats_taiko")]
public class UsersStatsTaiko : IUserStats
{
    public int Id { get; set; }
    public int PerformancePoints { get; set; }
    public int Level { get; set; }
    public int LevelProgress { get; set; }
    public long TotalScore { get; set; }
    public long TotalHits { get; set; }
    public int MaxCombo { get; set; }
    public long RankedScore { get; set; }
    public float Accuracy { get; set; }
}