using osu.Game.Online;
using OsuLazerServer.Database;
using OsuLazerServer.Multiplayer;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Services.Wiki;
using OsuLazerServer.SpectatorClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LazerContext>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR(c => c.EnableDetailedErrors = true).AddMessagePackProtocol(options => options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS);

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(args => args.Configuration = "localhost:6379");
builder.Services.AddScoped<ITokensService, TokenService>();
builder.Services.AddSingleton<IUserStorage, UserStorage>();
builder.Services.AddScoped<IBeatmapSetResolver, BeatmapSetResolverService>();
builder.Services.AddScoped<IWikiResolver, WikiResolverService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MultiplayerHub>("/multiplayer");
    endpoints.MapHub<SpectatorHub>("/spectator");
});
app.Run();