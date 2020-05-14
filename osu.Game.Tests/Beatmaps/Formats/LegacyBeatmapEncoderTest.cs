// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapEncoderTest
    {
        private static IEnumerable<string> allBeatmaps => TestResources.GetStore().GetAvailableResources().Where(res => res.EndsWith(".osu"));

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestEncodeDecodeStability(string name)
        {
            var decoded = decodeFromLegacy(TestResources.GetStore().GetStream(name));
            var decodedAfterEncode = decodeFromLegacy(encodeToLegacy(decoded));

            sort(decoded);
            sort(decodedAfterEncode);

            Assert.That(decodedAfterEncode.Serialize(), Is.EqualTo(decoded.Serialize()));
        }

        private void sort(IBeatmap beatmap)
        {
            // Sort control points to ensure a sane ordering, as they may be parsed in different orders. This works because each group contains only uniquely-typed control points.
            foreach (var g in beatmap.ControlPointInfo.Groups)
            {
                ArrayList.Adapter((IList)g.ControlPoints).Sort(
                    Comparer<ControlPoint>.Create((c1, c2) => string.Compare(c1.GetType().ToString(), c2.GetType().ToString(), StringComparison.Ordinal)));
            }
        }

        private IBeatmap decodeFromLegacy(Stream stream)
        {
            using (var reader = new LineBufferedReader(stream))
                return convert(new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(reader));
        }

        private Stream encodeToLegacy(IBeatmap beatmap)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(beatmap).Encode(writer);

            stream.Position = 0;

            return stream;
        }

        private IBeatmap convert(IBeatmap beatmap)
        {
            switch (beatmap.BeatmapInfo.RulesetID)
            {
                case 0:
                    beatmap.BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
                    break;

                case 1:
                    beatmap.BeatmapInfo.Ruleset = new TaikoRuleset().RulesetInfo;
                    break;

                case 2:
                    beatmap.BeatmapInfo.Ruleset = new CatchRuleset().RulesetInfo;
                    break;

                case 3:
                    beatmap.BeatmapInfo.Ruleset = new ManiaRuleset().RulesetInfo;
                    break;
            }

            return new TestWorkingBeatmap(beatmap).GetPlayableBeatmap(beatmap.BeatmapInfo.Ruleset);
        }

        private class TestWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public TestWorkingBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetTrack() => throw new NotImplementedException();
        }
    }
}
