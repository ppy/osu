// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class LegacySkinColourDecoderTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestDecodeSkinColours(bool hasColours)
        {
            var decoder = new LegacySkinColourDecoder();

            using (var resStream = TestResources.OpenResource(hasColours ? "skin.ini" : "skin-empty.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var comboColors = decoder.Decode(stream).ComboColours;

                List<Color4> expectedColors;
                if (hasColours)
                    expectedColors = new List<Color4>
                    {
                        new Color4(142, 199, 255, 255),
                        new Color4(255, 128, 128, 255),
                        new Color4(128, 255, 255, 255),
                        new Color4(100, 100, 100, 100),
                    };
                else
                    expectedColors = new DefaultSkin().Configuration.Colours.ComboColours;

                Assert.AreEqual(expectedColors.Count, comboColors.Count);
                for (int i = 0; i < expectedColors.Count; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        [Test]
        public void TestDecodeBeatmapColours()
        {
            var decoder = new LegacySkinColourDecoder();

            using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var comboColors = decoder.Decode(stream).ComboColours;

                Color4[] expectedColors =
                {
                    new Color4(142, 199, 255, 255),
                    new Color4(255, 128, 128, 255),
                    new Color4(128, 255, 255, 255),
                    new Color4(128, 255, 128, 255),
                    new Color4(255, 187, 255, 255),
                    new Color4(255, 177, 140, 255),
                    new Color4(100, 100, 100, 100),
                };
                Assert.AreEqual(expectedColors.Length, comboColors.Count);
                for (int i = 0; i < expectedColors.Length; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }
    }
}
