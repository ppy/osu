// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Skins
{
    [TestFixture]
    public class LegacySkinDecoderTest
    {
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
