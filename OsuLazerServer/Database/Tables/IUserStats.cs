using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OsuLazerServer.Database.Tables;

public interface IUserStats
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Required]
    public int Id { get; set; }
    
    [Column("perfomance_points")] public int PerfomancePoints { get; set; }
    [Column("level")] public int Level { get; set; }
    [Column("level_progress")] public int LevelProgress { get; set; }
    [Column("total_scope")] public long TotalScore { get; set; }
    [Column("total_hits")] public long TotalHits { get; set; }
    [Column("maximum_combo")] public int MaxCombo { get; set; }
    [Column("ranked_score")] public long RankedScore { get; set; }
    [Column("accuracy")] public float Accuracy { get; set; }
}