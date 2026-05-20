// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ManiaScoreMultiplierTest : RulesetScoreMultiplierTest
    {
        public ManiaScoreMultiplierTest()
            : base(new ManiaRuleset())
        {
        }

        private static readonly object[][] test_cases =
        [
            #region Difficulty Reduction

            [new Mod[] { new ManiaModEasy() }, 0.5],
            [new Mod[] { new ManiaModNoFail() }, 0.5],

            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new ManiaModHalfTime { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new ManiaModDaycore { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new ManiaModNoRelease() }, 0.9],

            #endregion

            #region Difficulty Increase

            [new Mod[] { new ManiaModHardRock() }, 1],
            [new Mod[] { new ManiaModSuddenDeath() }, 1],
            [new Mod[] { new ManiaModPerfect() }, 1],

            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.01 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.05 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.10 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.15 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.20 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.25 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.30 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.35 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.40 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.45 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.50 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.55 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.60 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.65 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.70 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.75 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.80 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.85 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.90 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 1.95 } } }, 1],
            [new Mod[] { new ManiaModDoubleTime { SpeedChange = { Value = 2.00 } } }, 1],

            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.01 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.05 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.10 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.15 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.20 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.25 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.30 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.35 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.40 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.45 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.50 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.55 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.60 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.65 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.70 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.75 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.80 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.85 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.90 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 1.95 } } }, 1],
            [new Mod[] { new ManiaModNightcore { SpeedChange = { Value = 2.00 } } }, 1],

            [new Mod[] { new ManiaModFadeIn() }, 1],
            [new Mod[] { new ManiaModHidden() }, 1],
            [new Mod[] { new ManiaModCover() }, 1],

            [new Mod[] { new ManiaModFlashlight() }, 1],
            [new Mod[] { new ModAccuracyChallenge() }, 1],

            #endregion

            #region Conversion

            [new Mod[] { new ManiaModRandom() }, 1],
            [new Mod[] { new ManiaModDualStages() }, 1],
            [new Mod[] { new ManiaModMirror() }, 1],
            [new Mod[] { new ManiaModDifficultyAdjust() }, 0.5],
            [new Mod[] { new ManiaModClassic() }, 0.96],
            [new Mod[] { new ManiaModInvert() }, 1],
            [new Mod[] { new ManiaModConstantSpeed() }, 0.9],
            [new Mod[] { new ManiaModHoldOff() }, 0.9],
            [new Mod[] { new ManiaModKey1() }, 0.9],
            [new Mod[] { new ManiaModKey2() }, 0.9],
            [new Mod[] { new ManiaModKey3() }, 0.9],
            [new Mod[] { new ManiaModKey4() }, 0.9],
            [new Mod[] { new ManiaModKey5() }, 0.9],
            [new Mod[] { new ManiaModKey6() }, 0.9],
            [new Mod[] { new ManiaModKey7() }, 0.9],
            [new Mod[] { new ManiaModKey8() }, 0.9],
            [new Mod[] { new ManiaModKey9() }, 0.9],
            [new Mod[] { new ManiaModKey10() }, 0.9],

            #endregion

            #region Automation

            [new Mod[] { new ManiaModAutoplay() }, 1],
            [new Mod[] { new ManiaModCinema() }, 1],

            #endregion

            #region Fun

            [new Mod[] { new ModWindUp() }, 0.5],
            [new Mod[] { new ModWindDown() }, 0.5],
            [new Mod[] { new ManiaModMuted() }, 1],
            [new Mod[] { new ModAdaptiveSpeed() }, 0.5],

            #endregion

            #region System

            [new Mod[] { new ManiaModScoreV2() }, 1],

            #endregion

            #region Combinations

            [new Mod[] { new ManiaModEasy(), new ManiaModKey4() }, 0.5 * 0.9]

            #endregion
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestMultipliers(Mod[] mods, double expectedMultiplier)
            => TestModCombination(mods, expectedMultiplier);
    }
}
