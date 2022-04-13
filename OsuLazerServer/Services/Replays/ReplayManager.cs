using System.Globalization;
using System.Text;
using osu.Framework.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using OsuLazerServer.Services.Beatmaps;
using OsuLazerServer.Services.Rulesets;
using OsuLazerServer.Services.Users;
using SharpCompress.Compressors.LZMA;
using SerializationWriter = osu.Game.IO.Legacy.SerializationWriter;
using OsuLazerServer.ThirdParty;

namespace OsuLazerServer.Services.Replays;

public class ReplayManager : IReplayManager, IServiceScope
{
    private IUserStorage _storage;
    private IBeatmapSetResolver _resolver;

    public IServiceProvider ServiceProvider { get; }

    public ReplayManager(IServiceProvider provider)
    {
        var storage = provider.CreateScope().ServiceProvider.GetService<IUserStorage>();
        var resolver = provider.CreateScope().ServiceProvider.GetService<IBeatmapSetResolver>();
        _storage = storage;
        _resolver = resolver;
        ServiceProvider = provider;
    }

    public Dictionary<int, Replay> Replays { get; set; } = new();

    public async Task WriteReplayData(int userId, int beatmapId, FrameDataBundle bundle, IList<APIMod> mods)
    {
        if (!Replays.TryGetValue(userId, out var replay))
        {
            Replays.Add(userId, new Replay
            {
                Frames = new List<LegacyReplayFrame>(),
                Mods = new(),
                BeatmapId = beatmapId,
                IsLegacy = true
            });
            replay = Replays[userId];
        }

        replay.Mods = mods.ToList();
        replay.BeatmapId = beatmapId;
        replay.Bundle = bundle;
        replay.Frames.AddRange(bundle.Frames);

        /*foreach (var frame in bundle.Frames)
        {

            if (replay.Frames.Count > 0)
                frame.Time = frame.Time - replay.Frames.Last().Time;
            else frame.Time = 1;
        }*/
    }

    public async Task<Stream> GetReplayLegacyDataAsync(int userId, ScoreInfo info)
    {
        if (!Replays.TryGetValue(userId, out var replay))
            return null;

        using var scope = ServiceProvider.CreateScope();

        var rulesetManager = scope.ServiceProvider.GetService<IRulesetManager>();
        var user = _storage.Users.Values.FirstOrDefault(u => u.Id == userId);
        var ms = new MemoryStream();
        var writer = new SerializationWriter(ms);

        var beatmap = await _resolver.FetchBeatmap(replay.BeatmapId);
        var ruleset = rulesetManager.GetRuleset(info.RulesetID);

        LegacyReplayFrame[] replayFrames = new LegacyReplayFrame[replay.Frames.Count];
        replay.Frames.CopyTo(replayFrames);

        var stats = replay.Bundle.Header.Statistics;
        writer.Write((byte) replay.RulesetId);
        writer.Write(30000000);
        writer.Write(beatmap.Checksum);
        writer.Write(user.Username);
        writer.Write(FormattableString.Invariant($"lazer-{user.Username}-{DateTime.Now}").ComputeMD5Hash());
        writer.Write((ushort) (info.GetCount300() ?? 0));
        writer.Write((ushort) (info.GetCount100() ?? 0));
        writer.Write((ushort) (info.GetCount50() ?? 0));
        writer.Write((ushort) (info.GetCountGeki() ?? 0));
        writer.Write((ushort) (info.GetCountKatu() ?? 0));
        writer.Write((ushort) (info.GetCountMiss() ?? 0));
        writer.Write((int)info.TotalScore);
        writer.Write((ushort) replay.Bundle.Header.MaxCombo);
        writer.Write((byte) replay.Bundle.Header.MaxCombo == beatmap.MaxCombo);
        writer.Write((int) ruleset.ConvertToLegacyMods(replay.Mods.Select(c => c.ToMod(ruleset)).ToArray())); // mods
        writer.Write(string.Empty);
        writer.Write(DateTime.Now);
        
        if (replay.Frames.Count > 0)
        {
            var frames = new StringBuilder();

            long lastFrameTime = 0;
            
            foreach (var frame in replayFrames)
            {
                frames.Append($"{(long)frame.Time - lastFrameTime}|{frame.Position.X}|{frame.Position.Y}|{(int)frame.ButtonState},");
                lastFrameTime = (long)frame.Time;
            }

            var content = new ASCIIEncoding().GetBytes(frames.ToString());
            using (var outStream = new MemoryStream())
            {
                using (var lzma = new LzmaStream(new LzmaEncoderProperties(false, 1 << 21, 255), false, outStream))
                {
                    outStream.Write(lzma.Properties);
                    
                    long fileSize = content.Length;
                    for (int i = 0; i < 8; i++)
                        outStream.WriteByte((byte)(fileSize >> (8 * i)));
                    
                    lzma.Write(content);
                }
                writer.WriteByteArray(outStream.ToArray());
            }
        }


        writer.Write(info.OnlineID);
        return ms;
    }

    public string Format(float value) => value.ToString(new CultureInfo(@"en-US", false).NumberFormat);
    

    public void ClearReplayFrames(int userId)
    {
        if (!Replays.TryGetValue(userId, out var replay))
            return;
        replay.Frames.Clear();
    }


    public void Dispose()
    {
    }
}