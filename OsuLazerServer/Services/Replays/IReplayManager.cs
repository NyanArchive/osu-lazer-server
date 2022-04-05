using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Shared.Serialization;

namespace OsuLazerServer.Services.Replays;

public class Replay
{
    public bool IsLegacy { get; set; }
    public List<LegacyReplayFrame> Frames { get; set; }
    public FrameDataBundle Bundle { get; set; }
    public int BeatmapId { get; set; }
    public List<APIMod> Mods { get; set; }
    public int RulesetId { get; set; }
}

public interface IReplayManager
{
    public Dictionary<int, Replay> Replays { get; set; }
    public Task WriteReplayData(int userId, int beatmapId, FrameDataBundle bundle, IList<APIMod> mods);
    public Task<Stream> GetReplayLegacyDataAsync(int userId, ScoreInfo info);
    public void ClearReplayFrames(int userId);
}