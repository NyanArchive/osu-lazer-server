using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;

namespace OsuLazerServer.Utils;

public class ProcessorWorkingBeatmap : WorkingBeatmap
{
    private readonly Beatmap _beatmap;

    public ProcessorWorkingBeatmap(Stream stream, int? beatmapId = null)
        : this(ReadFromStream(stream), beatmapId)
    {
    }

    private ProcessorWorkingBeatmap(Beatmap beatmap, int? beatmapId = null)
        : base(beatmap.BeatmapInfo, null)
    {
        _beatmap = beatmap;

        var ruleset = (RulesetId)beatmap.BeatmapInfo.Ruleset.OnlineID switch
        {
            RulesetId.Osu => new OsuRuleset().RulesetInfo,
            RulesetId.Taiko => new TaikoRuleset().RulesetInfo,
            RulesetId.Fruits => new CatchRuleset().RulesetInfo,
            RulesetId.Mania => new ManiaRuleset().RulesetInfo,
            _ => new OsuRuleset().RulesetInfo,
        };


        if (beatmapId.HasValue)
            beatmap.BeatmapInfo.OnlineID = beatmapId.Value;
    }

    private static Beatmap ReadFromFile(string filename)
    {
        using var stream = File.OpenRead(filename);
        using var streamReader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
    }

    private static Beatmap ReadFromStream(Stream stream)
    {
        using var streamReader = new LineBufferedReader(stream);

        return Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
    }

    protected override IBeatmap GetBeatmap() => _beatmap;
    protected override Texture GetBackground() => null;
    protected override Track GetBeatmapTrack() => null;
    protected override ISkin GetSkin() => null;

    public override Stream GetStream(string storagePath) => null;
}