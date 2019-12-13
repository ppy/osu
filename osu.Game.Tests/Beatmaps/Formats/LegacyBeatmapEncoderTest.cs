// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IO.Serialization;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyBeatmapEncoderTest
    {
        private const string normal = "Soleily - Renatus (Gamu) [Insane].osu";

        private static IEnumerable<string> allBeatmaps => TestResources.GetStore().GetAvailableResources().Where(res => res.EndsWith(".osu"));

        [TestCaseSource(nameof(allBeatmaps))]
        public void TestDecodeEncodedBeatmap(string name)
        {
            var decoded = decode(normal, out var encoded);

            Assert.That(decoded.HitObjects.Count, Is.EqualTo(encoded.HitObjects.Count));
            Assert.That(encoded.Serialize(), Is.EqualTo(decoded.Serialize()));
        }

        private Beatmap decode(string filename, out Beatmap encoded)
        {
            using (var stream = TestResources.OpenResource(filename))
            using (var sr = new LineBufferedReader(stream))
            {
                var legacyDecoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr);

                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var sr2 = new LineBufferedReader(ms))
                {
                    new LegacyBeatmapEncoder(legacyDecoded).Encode(sw);
                    sw.Flush();

                    ms.Position = 0;

                    encoded = new LegacyBeatmapDecoder { ApplyOffsets = false }.Decode(sr2);
                    return legacyDecoded;
                }
            }
        }
    }
}
