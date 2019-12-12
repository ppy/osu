// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osuTK.Graphics;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class LegacySkinDecoderTest
    {
        public void TestDecodeSkinColours()
        {
            var decoder = new LegacySkinDecoder();

            using (var resStream = TestResources.OpenResource("skin.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var comboColors = decoder.Decode(stream).ComboColours;
                var expectedColors = new List<Color4>
                {
                    new Color4(142, 199, 255, 255),
                    new Color4(255, 128, 128, 255),
                    new Color4(128, 255, 255, 255),
                    new Color4(100, 100, 100, 100),
                };

                Assert.AreEqual(expectedColors.Count, comboColors.Count);
                for (int i = 0; i < expectedColors.Count; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        public void TestDecodeEmptySkinColours()
        {
            var decoder = new LegacySkinDecoder();

            using (var resStream = TestResources.OpenResource("skin-empty.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var comboColors = decoder.Decode(stream).ComboColours;
                var expectedColors = SkinConfiguration.DefaultComboColours;

                Assert.AreEqual(expectedColors.Count, comboColors.Count);
                for (int i = 0; i < expectedColors.Count; i++)
                    Assert.AreEqual(expectedColors[i], comboColors[i]);
            }
        }

        public void TestDecodeEmptySkinColoursNoFallback()
        {
            var decoder = new LegacySkinDecoder();

            using (var resStream = TestResources.OpenResource("skin-empty.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var skinConfiguration = decoder.Decode(stream);
                skinConfiguration.AllowDefaultComboColoursFallback = false;
                Assert.IsNull(skinConfiguration.ComboColours);
            }
        }

        [Test]
        public void TestDecodeGeneral()
        {
            var decoder = new LegacySkinDecoder();

            using (var resStream = TestResources.OpenResource("skin.ini"))
            using (var stream = new LineBufferedReader(resStream))
            {
                var config = decoder.Decode(stream);

                Assert.AreEqual("test skin", config.SkinInfo.Name);
                Assert.AreEqual("TestValue", config.ConfigDictionary["TestLookup"]);
            }
        }
    }
}
