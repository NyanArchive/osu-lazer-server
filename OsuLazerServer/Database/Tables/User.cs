using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Nager.Country;
using OsuLazerServer.Database.Tables.Scores;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Users;
using OsuLazerServer.Services.Users;
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
    [Column("is_admin")]
    [Required]
    [DefaultValue(false)]
    public bool IsAdmin { get; set; }


    public async Task FetchUserStats()
    {
        var ctx = new LazerContext { Database = { AutoTransactionsEnabled = false}};
        StatsOsu = await ctx.OsuStats.FirstAsync(s => s.Id == Id);
        StatsTaiko = await ctx.TaikoStats.FirstAsync(s => s.Id == Id);
        StatsMania = await ctx.ManiaStats.FirstAsync(s => s.Id == Id);
        StatsFruits = await ctx.FruitsStats.FirstAsync(s => s.Id == Id);
    }

    public IUserStats? FetchStats(string mode) => ModeUtils.FetchUserStats(new LazerContext(), mode, Id);

    public IUserStats GetStats(string mode)
    {
        switch (mode)
        {
            case "osu":
                return StatsOsu;
            case "taiko":
                return StatsTaiko;
            case "fruits":
                return StatsFruits;
            case "mania":
                return StatsMania;
            default:
                return StatsOsu;
        }
    }


    public APIUser ToOsuUser(string mode, IUserStorage? storage = null)
    {
        var context = new LazerContext();
        var stats = ModeUtils.FetchUserStats(context, mode, Id);
        return new APIUser
        {
            Username = Username,
            Id = Id,
            IsOnline = storage?.Users.Values.Any(u => u.Id == Id)??false,
            RankHistory = new RankHistory
            {
                Data = new List<int>(),
                Mode = "osu"
            },
            IsSupporter = true,
            JoinDate = JoinedAt,
            Website = "https://google.com",
            Country =  new Country
            {
                Code = Country,
                Name = new CountryProvider()?.GetCountry(Country)?.CommonName??"Unknown"
            },
            CountryCode = Country,
            Cover = new Cover
            {
                Id = null,
                Url = "https://media.discordapp.net/attachments/951904126164410388/954157898102087710/FNtSlHHakAEETLx.jpg",
                CustomUrl = "https://media.discordapp.net/attachments/951904126164410388/954157898102087710/FNtSlHHakAEETLx.jpg"
            },
            Statistics = new Statistics
            {
                Level = new Level
                {
                    Current = stats?.Level??0,
                    Progress = stats?.LevelProgress??0
                },
                TotalHits = (int) (stats?.TotalHits ?? 0),
                TotalScore = stats?.TotalScore??0,
                CountryRank = 1,
                GlobalRank = storage is not null ? storage.GetUserRank(Id, mode switch
                {
                    "osu" => 0,
                    "taiko" => 1,
                    "fruits" => 2,
                    "mania" => 3
                }, false).GetAwaiter().GetResult() : 0,
                GradeCounts = new GradeCounts
                {
                    A = context.Scores.Count(s => s.Passed && s.Rank == ScoreRank.A && s.UserId == Id),
                    S = context.Scores.Count(s => s.Passed && s.Rank == ScoreRank.S && s.UserId == Id),
                    Sh = context.Scores.Count(s => s.Passed && s.Rank == ScoreRank.SH && s.UserId == Id),
                    Ss = context.Scores.Count(s => s.Passed && s.Rank == ScoreRank.X && s.UserId == Id),
                    Ssh = context.Scores.Count(s => s.Passed && s.Rank == ScoreRank.XH && s.UserId == Id),
                },
                RankedScore = stats?.RankedScore??0,
                PerfomancePoints = stats?.PerfomancePoints??0,
                ReplaysWatchedByOthers = ReplaysWatches,
                HitAccuracy = stats?.Accuracy??0 * 100,
                IsRanked = true,
                MaximumCombo = stats?.MaxCombo??0,
                PlayCount = PlayCount,
                PlayTime = 0,
            },
            ScoresBestCount = context.Scores.Where(c => c.Passed && c.Status == DbScoreStatus.BEST && c.UserId == Id).Take(50).Count(),
            ScoresRecentCount = context.Scores.Where(c => c.Passed && c.UserId == Id).Take(50).Count(),
            IsActive = true,
            ProfileOrder = new List<string>
            {
                "me",
                "top_ranks",
                "recent_activity"
            }
        };
    }
}