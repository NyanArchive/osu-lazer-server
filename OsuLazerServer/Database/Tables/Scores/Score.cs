using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using osu.Game.Online.API;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Scores;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Utils;
using LazerStatus = OsuLazerServer.Models.Response.Beatmaps.BeatmapOnlineStatus;
namespace OsuLazerServer.Database.Tables.Scores;


[Table("scores")]
public class DbScore
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public int Id { get; set; }

    [Column("user_id")] [Required] public int UserId { get; set; }

    public async Task<User?> GetUserAsync(LazerContext ctx)
    {
        return await ctx.Users.FirstOrDefaultAsync(s => s.Id == UserId);
    }
    public User? GetUser(LazerContext ctx)
    {
        return ctx.Users.FirstOrDefault(s => s.Id == UserId);
    }

    
    [Column("beatmap_id")] [Required] public int BeatmapId { get; set; }
    
    [Column("rank")] [Required] public ScoreRank Rank { get; set; }

    [Column("total_score")] public long TotalScore { get; set; }
    
    [Column("accuracy")] public double Accuracy { get; set; }
    
    [Column("perfomance_points")] public double PerfomancePoints { get; set; }
    
    [Column("max_combo")] public int MaxCombo { get; set; }
    
    [Column("ruleset_id")] public int RuleSetId { get; set; }
    
    [Column("mods")] public List<string> Mods { get; set; }
    
    [Column("submitted_at")]
    public DateTimeOffset SubmittedAt { get; set; }
    
    [Column("statistics")]
    public string Statistics { get; set; }

    [Column("submitted_in")]
    public int SubmittedIn { get; set; }
    [Column("submittion_playlist")]
    public int SubmittionPlaylist { get; set; }

    [Column("passed")]
    public bool Passed { get; set; }
    public async Task<APIScore> ToOsuScore(IBeatmapSetResolver? resolver = null)
    {
        var ctx = new LazerContext();
        var beatmap = resolver is not null ? (await resolver.FetchBeatmap(BeatmapId)) : null;
        return new APIScore
        {
            Accuracy = Accuracy,
            Beatmap = beatmap is not null ? await beatmap?.ToOsu() : null,
            beatmapSet = resolver is not null ? (await resolver.FetchSetAsync(beatmap.BeatmapsetId)).ToBeatmapSet() : null,
            Date = SubmittedAt,
            Rank = ModeUtils.CalculateRank(this),
            Statistics = HitResultStats.FromJson(Statistics).ToOsu(),
            User =
                await (await GetUserAsync(ctx))?.ToOsuUser(Enum.GetName(typeof(RulesetId), RuleSetId) ?? "osu") ??
                new APIUser {Id = 1, Username = "Bancho bot"},
            HasReplay = beatmap?.Status != "pending" &&
                        beatmap?.Status != "graveyard"&&
                        beatmap?.Status != "WIP" &&
                        beatmap?.Status != "None",
            Mods = Mods.ToArray(),
            MaxCombo = MaxCombo,
            PP = PerfomancePoints,
            TotalScore = TotalScore,
            OnlineID = Id,
            RulesetID = RuleSetId
        };
    }
    
    [Column("status")]
    public DbScoreStatus Status { get; set; }
}

public enum DbScoreStatus
{
    BEST,
    OUTDATED,
    FIRST
}