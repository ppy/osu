// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchSkinColourDecodingTest
    {
        [Test]
        public void TestCatchSkinColourDecoding()
        {
            var store = new NamespacedResourceStore<byte[]>(new DllResourceStore(GetType().Assembly), "Resources/special-skin");
            var rawSkin = new TestLegacySkin(new SkinInfo { Name = "special-skin" }, store);
            var skinSource = new SkinProvidingContainer(rawSkin);
            var skin = new CatchLegacySkinTransformer(skinSource);

            Assert.AreEqual(new Color4(232, 185, 35, 255), skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value);
            Assert.AreEqual(new Color4(232, 74, 35, 255), skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value);
            Assert.AreEqual(new Color4(0, 255, 255, 255), skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashFruit)?.Value);
        }

        private class TestLegacySkin : LegacySkin
        {
            public TestLegacySkin(SkinInfo skin, IResourceStore<byte[]> storage)
                // Bypass LegacySkinResourceStore to avoid returning null for retrieving files due to bad skin info (SkinInfo.Files = null).
                : base(skin, null, storage)
            {
            }
        }
    }
}
