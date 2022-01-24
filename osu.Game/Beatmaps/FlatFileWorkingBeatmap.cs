// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Skinning;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="WorkingBeatmap"/> which can be constructed directly from a .osu file, providing an implementation for
    /// <see cref="WorkingBeatmap.GetPlayableBeatmap(osu.Game.Rulesets.IRulesetInfo,System.Collections.Generic.IReadOnlyList{osu.Game.Rulesets.Mods.Mod})"/>.
    /// </summary>
    public class FlatFileWorkingBeatmap : WorkingBeatmap
    {
        private readonly Beatmap beatmap;

        public FlatFileWorkingBeatmap(string file, Func<int, Ruleset> rulesetProvider, int? beatmapId = null)
            : this(readFromFile(file), rulesetProvider, beatmapId)
        {
        }

        private FlatFileWorkingBeatmap(Beatmap beatmap, Func<int, Ruleset> rulesetProvider, int? beatmapId = null)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;

            beatmap.BeatmapInfo.Ruleset = rulesetProvider(beatmap.BeatmapInfo.RulesetID).RulesetInfo;

            if (beatmapId.HasValue)
                beatmap.BeatmapInfo.OnlineID = beatmapId.Value;
        }

        private static Beatmap readFromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            using (var reader = new LineBufferedReader(stream))
                return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => throw new NotImplementedException();
        protected override Track GetBeatmapTrack() => throw new NotImplementedException();
        protected internal override ISkin GetSkin() => throw new NotImplementedException();
        public override Stream GetStream(string storagePath) => throw new NotImplementedException();
    }
}
