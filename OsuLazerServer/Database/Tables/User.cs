using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Nager.Country;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Users;
using OsuLazerServer.Utils;
using Country = OsuLazerServer.Models.Country;

namespace OsuLazerServer.Database.Tables;

[Table("users")]
public class User
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Required]
    public int Id { get; set; }

    [Column("username")] [Required] public string Username { get; set; }

    [Column("email")] [Required] public string Email { get; set; }

    [Column("nickname_history")]
    [Required]
    public string[] NicknameHistory { get; set; }

    [Column("banned")] [Required] public bool Banned { get; set; }

    [Column("password")] [Required] public string Password { get; set; }

    public string UsernameSafe => Username.ToLower().Replace(" ", "_");

    [Column("country")] [Required] public string Country { get; set; }

    [Column("play_count")] public int PlayCount { get; set; }

    [Column("replays_watched")] public int ReplaysWatches { get; set; }

    [Column("osu_stats")] public UsersStatsOsu StatsOsu { get; set; }
    [Column("taiko_stats")] public UsersStatsTaiko StatsTaiko { get; set; }
    [Column("fruits_stats")] public UsersStatsFruits StatsFruits { get; set; }
    [Column("mania_stats")] public UsersStatsMania StatsMania { get; set; }
    
    [Column("joined_at")] public DateTime JoinedAt { get; set; }


    public async Task FetchUserStats(LazerContext context)
    {
        StatsOsu = await context.OsuStats.FirstAsync(s => s.Id == Id);
        StatsTaiko = await context.TaikoStats.FirstAsync(s => s.Id == Id);
        StatsMania = await context.ManiaStats.FirstAsync(s => s.Id == Id);
        StatsFruits = await context.FruitsStats.FirstAsync(s => s.Id == Id);
    }


    public async Task<APIUser> ToOsuUser(string mode, LazerContext context)
    {
        var stats = await ModeUtils.FetchUserStats(context, mode, Id);
        return new APIUser
        {
            Username = Username,
            Id = Id,
            IsSupporter = true,
            JoinDate = JoinedAt,
            Website = "https://google.com",
            Country =  new Country
            {
                Code = Country,
                Name = new CountryProvider().GetCountry(Country).CommonName
            },
            CountryCode = Country,
            Cover = new Cover
            {
                Id = null,
                Url = "https://media.discordapp.net/attachments/805142641427218452/933105146404143144/unknown.png",
                CustomUrl = "https://media.discordapp.net/attachments/805142641427218452/933105146404143144/unknown.png"
            },
            Statistics = new Statistics
            {
                Level = new Level
                {
                    Current = stats.Level,
                    Progress = stats.LevelProgress
                },
                TotalHits = (int) stats.TotalHits,
                TotalScore = stats.TotalScore,
                CountryRank = 1,
                GlobalRank = 1,
                GradeCounts = new GradeCounts
                {
                    A = await context.Scores.CountAsync(s => s.Passed && s.Rank == ScoreRank.A && s.UserId == Id),
                    S = await context.Scores.CountAsync(s => s.Passed && s.Rank == ScoreRank.S && s.UserId == Id),
                    Sh = await context.Scores.CountAsync(s => s.Passed && s.Rank == ScoreRank.SH && s.UserId == Id),
                    Ss = await context.Scores.CountAsync(s => s.Passed && s.Rank == ScoreRank.X && s.UserId == Id),
                    Ssh = await context.Scores.CountAsync(s => s.Passed && s.Rank == ScoreRank.XH && s.UserId == Id),
                },
                RankedScore = stats.RankedScore,
                PP = stats.PerfomancePoints,
                ReplaysWatchedByOthers = ReplaysWatches,
                HitAccuracy = stats.Accuracy * 100,
                IsRanked = true,
                MaximumCombo = stats.MaxCombo,
                PlayCount = PlayCount,
                PlayTime = 0,
            },
            ScoresBestCount = await context.Scores.Where(c => c.Passed && c.Status == DbScoreStatus.BEST && c.UserId == Id).Take(50).CountAsync(),
            ScoresRecentCount = await context.Scores.Where(c => c.Passed && c.UserId == Id).Take(50).CountAsync()
        };
    }
}