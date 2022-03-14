using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OsuLazerServer.Database;
using OsuLazerServer.Multiplayer;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Users;
using OsuLazerServer.SpectatorClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LazerContext>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR().AddMessagePackProtocol();
builder.Services.AddScoped<ITokensService, TokenService>();
builder.Services.AddSingleton<IUserStorage, UserStorage>();
builder.Services.AddScoped<IBeatmapSetResolver, BeatmapSetResolverService>();

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