using BackgroundQueue;
using BackgroundQueue.Generic;
using osu.Game.Online;
using OsuLazerServer.Database;
using OsuLazerServer.Multiplayer;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Commands;
using OsuLazerServer.Services.Users;
using OsuLazerServer.Services.Wiki;
using OsuLazerServer.SpectatorClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LazerContext>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR()
    .AddMessagePackProtocol(options => options.SerializerOptions = SignalRUnionWorkaroundResolver.OPTIONS); 
builder.Services.AddSentry();


//Should work?
builder.Services.AddMemoryCache();

builder.WebHost.UseSentry(o =>
{
    o.Dsn = "https://d0463415132a4afa8b7940cccd2b7243@o1175060.ingest.sentry.io/6271646";
    // When configuring for the first time, to see what the SDK is doing:
    o.Debug = false;
    // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
    // We recommend adjusting this value in production.
    o.TracesSampleRate = 1.0;
});
#if !DEBUG
builder.Services.AddStackExchangeRedisCache(args => args.Configuration = Environment.GetEnvironmentVariable("REDIS_URL"));
#else
builder.Services.AddStackExchangeRedisCache(args => args.Configuration = "localhost:6379");
#endif
builder.Services.AddScoped<ITokensService, TokenService>();
builder.Services.AddSingleton<IUserStorage, UserStorage>();
builder.Services.AddSingleton<IBeatmapSetResolver, BeatmapSetResolverService>();
builder.Services.AddScoped<IWikiResolver, WikiResolverService>();
builder.Services.AddSingleton<ICommandManager, CommandManagerService>();
builder.Services.AddBackgroundTaskQueue();
builder.Services.AddBackgroundResultQueue();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}


app.UseRouting();
app.MapControllers();
app.UseWebSockets();
app.UseSentryTracing();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MultiplayerHub>("/multiplayer");
    endpoints.MapHub<SpectatorHub>("/spectator");
});
app.Run();