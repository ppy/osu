// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
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
        public void TestBeatmap(string name)
        {
            var decoded = decode(name, out var encoded);

            Assert.That(decoded.HitObjects.Count, Is.EqualTo(encoded.HitObjects.Count));
            Assert.That(encoded.Serialize(), Is.EqualTo(decoded.Serialize()));
        }

        private IBeatmap decode(string filename, out IBeatmap encoded)
        {
            using (var stream = TestResources.GetStore().GetStream(filename))
            using (var sr = new LineBufferedReader(stream))
            {
                var legacyDecoded = convert(new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr));

                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var sr2 = new LineBufferedReader(ms, true))
                {
                    new LegacyBeatmapEncoder(legacyDecoded).Encode(sw);

                    sw.Flush();
                    ms.Position = 0;

                    encoded = convert(new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr2));

                    return legacyDecoded;
                }
            }
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
