// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModDoubleTime : ModTestScene
    {
        public TestSceneOsuModDoubleTime()
            : base(new OsuRuleset())
        {
        }

        [TestCase(0.5)]
        [TestCase(1.01)]
        [TestCase(1.5)]
        [TestCase(2)]
        [TestCase(5)]
        public void TestSpeedChangeCustomisation(double rate)
        {
            var mod = new OsuModDoubleTime { SpeedChange = { Value = rate } };

            CreateModTest(new ModTestData
            {
                Mod = mod,
                PassCondition = () => Player.ScoreProcessor.JudgedHits >= 2 &&
                                      Precision.AlmostEquals(Player.GameplayClockContainer.GameplayClock.Rate, mod.SpeedChange.Value)
            });
        }
    }
}
