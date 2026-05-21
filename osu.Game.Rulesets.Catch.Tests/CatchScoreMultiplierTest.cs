// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class CatchScoreMultiplierTest : RulesetScoreMultiplierTest
    {
        public CatchScoreMultiplierTest()
            : base(new CatchRuleset())
        {
        }

        [Test]
        public void TestFlashlightOnNonDefaultSettings()
            => TestModCombination([new CatchModFlashlight { ComboBasedSize = { Value = false } }]);

        [Test]
        public void TestHalfTimeSpeeds([Values(0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99)] double speedChange)
            => TestModCombination([new CatchModHalfTime { SpeedChange = { Value = speedChange } }]);

        [Test]
        public void TestDaycoreSpeeds([Values(0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 0.99)] double speedChange)
            => TestModCombination([new CatchModDaycore { SpeedChange = { Value = speedChange } }]);

        [Test]
        public void TestDoubleTimeSpeeds([Values(1.01, 1.05, 1.1, 1.15, 1.2, 1.25, 1.3, 1.35, 1.4, 1.45, 1.5, 1.55, 1.6, 1.65, 1.7, 1.75, 1.8, 1.85, 1.9, 1.95, 2)] double speedChange)
            => TestModCombination([new CatchModDoubleTime { SpeedChange = { Value = speedChange } }]);

        [Test]
        public void TestNightcoreSpeeds([Values(1.01, 1.05, 1.1, 1.15, 1.2, 1.25, 1.3, 1.35, 1.4, 1.45, 1.5, 1.55, 1.6, 1.65, 1.7, 1.75, 1.8, 1.85, 1.9, 1.95, 2)] double speedChange)
            => TestModCombination([new CatchModNightcore { SpeedChange = { Value = speedChange } }]);

        [Test]
        public void TestMultiplicativeCombination()
            => TestModCombination([new CatchModHidden(), new CatchModHardRock()]);
    }
}
