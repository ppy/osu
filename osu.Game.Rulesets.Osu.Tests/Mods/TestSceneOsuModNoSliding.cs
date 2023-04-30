// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModNoSliding : OsuModTestScene
    {
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(8)]
        public void TestHoldOff(int divisor)
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoSliding
                {
                    BeatDivisor = { Value = divisor }
                },
                PassCondition = () => true,
                Autoplay = false,
            });
        }
    }
}
