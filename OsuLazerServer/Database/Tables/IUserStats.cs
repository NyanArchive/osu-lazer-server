using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OsuLazerServer.Models;
using OsuLazerServer.Utils;

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

    public Statistics ToOsu(string mode) => new Statistics
    {
        Level = new Level
        {
            Current = 0,
            Progress = 0
        },
        CountryRank = 0,
        GlobalRank = ModeUtils.GetRank(mode, Id),
        HitAccuracy = Accuracy * 100,
        IsRanked = true,
        MaximumCombo = MaxCombo,
        PlayCount = 0,
        PlayTime = 0,
        GradeCounts = new GradeCounts
        {
            A = 0,
            S = 0,
            Sh = 0,
            Ss = 0,
            Ssh = 0
        },
        PP = PerfomancePoints,
        RankedScore = RankedScore,
        TotalHits = (int) TotalHits,
        TotalScore = TotalScore,
        ReplaysWatchedByOthers = 0,
        Rank = new Rank
        {
            Country = 0
        }
    };
}