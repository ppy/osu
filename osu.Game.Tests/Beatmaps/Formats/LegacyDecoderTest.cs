// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyDecoderTest
    {
        [Test]
        public void TestDecodeComments()
        {
            var decoder = new LineLoggingDecoder(14);

            using (var resStream = TestResources.OpenResource("comments.osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                decoder.Decode(stream);

                Assert.That(decoder.ParsedLines, Has.None.EqualTo("// Combo1: 0, 0, 0"));
                Assert.That(decoder.ParsedLines, Has.None.EqualTo("//Combo2: 0, 0, 0"));
                Assert.That(decoder.ParsedLines, Has.None.EqualTo(" // Combo3: 0, 0, 0"));
                Assert.That(decoder.ParsedLines, Has.One.EqualTo("Combo1: 100, 100, 100 // Comment at end of line"));
            }
        }

        private class LineLoggingDecoder : LegacyDecoder<TestModel>
        {
            public readonly List<string> ParsedLines = new List<string>();

            public LineLoggingDecoder(int version)
                : base(version)
            {
            }

            protected override bool ShouldSkipLine(string line)
            {
                var result = base.ShouldSkipLine(line);

                if (!result)
                    ParsedLines.Add(line);

                return result;
            }
        }

        private class TestModel
        {
        }
    }
}
