// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModZoomed : OsuModTestScene
    {
        [Test]
        public void TestBasic() => CreateModTest(new ModTestData
        {
            Mod = new OsuModZoomed(),
            PassCondition = () => true,
            Autoplay = false,
        });

        [Test]
        public void TestZeroDelay() => CreateModTest(new ModTestData
        {
            Mod = new OsuModZoomed
            {
                FinalZoom = { Value = 2 },
                FinalZoomCombo = { Value = 0 },
                MovementDelay = { Value = 0 }
            },
            PassCondition = () => true,
            Autoplay = true,
        });
    }
}
