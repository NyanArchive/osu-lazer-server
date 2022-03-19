using MessagePack;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;

namespace OsuLazerServer.SpectatorClient;

[MessagePackObject(true)]
public class ClientState
{

    [Key(0)]
    public uint BeatmapId { get; set; }
    [Key(1)]
    public uint RuleSetId { get; set; }
    [Key(2)]
    public IEnumerable<APIMod> Mods { get; set; } = Enumerable.Empty<APIMod>();
    [Key(3)]
    public SpectatedUserState State { get; set; }


}