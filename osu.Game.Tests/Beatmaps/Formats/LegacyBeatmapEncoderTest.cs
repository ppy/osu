// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets;
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
        private const string normal = "Soleily - Renatus (Gamu) [Insane].osu";

        private static IEnumerable<string> allBeatmaps => TestResources.GetStore().GetAvailableResources().Where(res => res.EndsWith(".osu"));

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestBeatmap(string name)
        {
            var decoded = decode(name, out var encoded);

            Assert.That(decoded.HitObjects.Count, Is.EqualTo(encoded.HitObjects.Count));

            string encodedSerialised = encoded.Serialize();
            string decodedSerialised = decoded.Serialize();

            Assert.That(encodedSerialised, Is.EqualTo(decodedSerialised));
        }

        private Beatmap decode(string filename, out Beatmap encoded)
        {
            using (var stream = TestResources.GetStore().GetStream(filename))
            using (var sr = new LineBufferedReader(stream))
            {
                var legacyDecoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr);

                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var sr2 = new LineBufferedReader(ms))
                {
                    RulesetInfo ruleset = null;

                    switch (legacyDecoded.BeatmapInfo.RulesetID)
                    {
                        case 0:
                            ruleset = new OsuRuleset().RulesetInfo;
                            break;

                        case 1:
                            ruleset = new TaikoRuleset().RulesetInfo;
                            break;

                        case 2:
                            ruleset = new CatchRuleset().RulesetInfo;
                            break;

                        case 3:
                            ruleset = new ManiaRuleset().RulesetInfo;
                            break;
                    }

                    var converted = new TestWorkingBeatmap(legacyDecoded).GetPlayableBeatmap(ruleset);

                    new LegacyBeatmapEncoder(converted).Encode(sw);

                    sw.Flush();
                    ms.Position = 0;

                    encoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr2);
                    return legacyDecoded;
                }
            }
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

            protected override Texture GetBackground() => throw new System.NotImplementedException();

            protected override Track GetTrack() => throw new System.NotImplementedException();
        }
    }
}
