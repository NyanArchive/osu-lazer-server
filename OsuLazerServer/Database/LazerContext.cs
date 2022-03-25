using Microsoft.EntityFrameworkCore;
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Database.Tables.Scores;

namespace OsuLazerServer.Database;

public class LazerContext : DbContext
{

    public DbSet<User> Users { get; set; }
    public DbSet<UsersStatsOsu> OsuStats { get; set; }
    public DbSet<UsersStatsTaiko> TaikoStats { get; set; }
    public DbSet<UsersStatsFruits> FruitsStats { get; set; }
    public DbSet<UsersStatsMania> ManiaStats { get; set; }
    public DbSet<DbScore> Scores { get;set; }
    public DbSet<Channel> Channels { get; set; }

    public LazerContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if !DEBUG
        optionsBuilder.UseNpgsql(
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NANE")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASS")}");
#else
        optionsBuilder.UseNpgsql(
            $"Host=localhost;Port=5432;Database=lazer;Username=postgres;Password=123321");
#endif

    }
    
    public async Task<User> CreateBot()
    {
        if (await Users.FirstOrDefaultAsync(u => u.Id == 1) is null)
        {
            var entity = await Users.AddAsync(new User
            {
                Username = "Oleg",
                Email = "admin@ppy.sh",
                Country = "UA",
                Password = "",
                NicknameHistory = new string[] {},
                PlayCount = 0,
                ReplaysWatches = 0,
                StatsFruits = new UsersStatsFruits(),
                StatsMania = new UsersStatsMania(),
                StatsOsu = new UsersStatsOsu(),
                StatsTaiko = new UsersStatsTaiko(),
                JoinedAt = DateTime.UtcNow,
                Banned = true
            });

            await SaveChangesAsync();
        }

        if (!(await Channels.AnyAsync()))
        {
            //Creating osu! channel
            await InitializeChannels();
        }

        return new User();
    }

    private async Task InitializeChannels()
    {

        await Channels.AddAsync(new Channel
        {
            Description = "Main channel",
            AllowedWrite = true,
            Name = "osu",
            Type = ChannelType.PUBLIC
        });


        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}