// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osuTK.Graphics;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class LegacyManiaSkinDecoderTest
    {
        [Test]
        public void TestParseSingleConfig()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-single.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].Keys, Is.EqualTo(4));
                Assert.That(configs[0].ColumnWidth, Is.EquivalentTo(new float[] { 16, 16, 16, 16 }));
                Assert.That(configs[0].HitPosition, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestParseMultipleConfig()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-multiple.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(2));

                Assert.That(configs[0].Keys, Is.EqualTo(4));
                Assert.That(configs[0].ColumnWidth, Is.EquivalentTo(new float[] { 16, 16, 16, 16 }));
                Assert.That(configs[0].HitPosition, Is.EqualTo(16));

                Assert.That(configs[1].Keys, Is.EqualTo(2));
                Assert.That(configs[1].ColumnWidth, Is.EquivalentTo(new float[] { 32, 32 }));
                Assert.That(configs[1].HitPosition, Is.EqualTo(32));
            }
        }

        [Test]
        public void TestParseDuplicateConfig()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-single.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].Keys, Is.EqualTo(4));
                Assert.That(configs[0].ColumnWidth, Is.EquivalentTo(new float[] { 16, 16, 16, 16 }));
                Assert.That(configs[0].HitPosition, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestParseWithUnnecessaryExtraData()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-extra-data.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].Keys, Is.EqualTo(4));
                Assert.That(configs[0].ColumnWidth, Is.EquivalentTo(new float[] { 16, 16, 16, 16 }));
                Assert.That(configs[0].HitPosition, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestParseColours()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-colours.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].CustomColours, Contains.Key("ColourBarline").And.ContainValue(new Color4(50, 50, 50, 50)));
            }
        }

        [Test]
        public void TestMinimumColumnWidthFallsBackWhenZeroIsProvided()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-zero-minwidth.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].MinimumColumnWidth, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestParseArrayWithSomeEmptyElements()
        {
            var decoder = new LegacyManiaSkinDecoder();

            using (var resStream = TestResources.OpenResource("mania-skin-broken-array.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var configs = decoder.Decode(stream);

                Assert.That(configs.Count, Is.EqualTo(1));
                Assert.That(configs[0].ColumnLineWidth[0], Is.EqualTo(3));
                Assert.That(configs[0].ColumnLineWidth[1], Is.EqualTo(0)); // malformed entry, should be parsed as zero
                Assert.That(configs[0].ColumnLineWidth[2], Is.EqualTo(3));
                Assert.That(configs[0].ColumnLineWidth[3], Is.EqualTo(3));
                Assert.That(configs[0].ColumnLineWidth[4], Is.EqualTo(3));
            }
        }
    }
}
