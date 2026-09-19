// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Scoring;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TaikoScoreMultiplierTest : RulesetScoreMultiplierTest
    {
        public TaikoScoreMultiplierTest()
            : base(new TaikoRuleset())
        {
        }

        private static readonly object[][] test_cases =
        [
            #region Difficulty Reduction

            [new Mod[] { new TaikoModEasy() }, 0.5],
            [new Mod[] { new TaikoModNoFail() }, 0.5],

            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new TaikoModHalfTime { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new TaikoModDaycore { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new TaikoModSimplifiedRhythm() }, 0.6],

            #endregion

            #region Difficulty Increase

            [new Mod[] { new TaikoModHardRock() }, 1.06],
            [new Mod[] { new TaikoModSuddenDeath() }, 1],
            [new Mod[] { new TaikoModPerfect() }, 1],

            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new TaikoModDoubleTime { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new TaikoModNightcore { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new TaikoModHidden() }, 1.06],

            [new Mod[] { new TaikoModFlashlight() }, 1.12],
            [new Mod[] { new TaikoModFlashlight { ComboBasedSize = { Value = false } } }, 1],

            [new Mod[] { new ModAccuracyChallenge() }, 1],

            #endregion

            #region Conversion

            [new Mod[] { new TaikoModRandom() }, 1],
            [new Mod[] { new TaikoModDifficultyAdjust() }, 0.5],
            [new Mod[] { new TaikoModClassic() }, 1],
            [new Mod[] { new TaikoModSwap() }, 1],
            [new Mod[] { new TaikoModSingleTap() }, 1],
            [new Mod[] { new TaikoModConstantSpeed() }, 0.9],

            #endregion

            #region Automation

            [new Mod[] { new TaikoModAutoplay() }, 1],
            [new Mod[] { new TaikoModCinema() }, 1],
            [new Mod[] { new TaikoModRelax() }, 0.1],

            #endregion

            #region Fun

            [new Mod[] { new ModWindUp() }, 0.5],
            [new Mod[] { new ModWindDown() }, 0.5],
            [new Mod[] { new TaikoModMuted() }, 1],
            [new Mod[] { new ModAdaptiveSpeed() }, 0.5],

            #endregion

            #region System

            [new Mod[] { new ModScoreV2() }, 1],

            #endregion

            #region Combinations

            [new Mod[] { new TaikoModHidden(), new TaikoModHardRock() }, 1.06 * 1.06]

            #endregion
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestMultipliers(Mod[] mods, double expectedMultiplier)
            => TestModCombination(mods, expectedMultiplier);

        [TestCase(30000001, 0.96)]
        [TestCase(30000009, 0.96)]
        [TestCase(30000016, 0.96)]
        [TestCase(30000017, 1)]
        [TestCase(null, 1)]
        public void TestClassicMultiplierVersioning(int? totalScoreVersion, double expectedMultiplier)
        {
            var scoreInfo = totalScoreVersion != null ? new ScoreInfo { TotalScoreVersion = totalScoreVersion.Value } : null;
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty(), scoreInfo));
            Assert.That(calculator.CalculateFor([new TaikoModClassic()]), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }
    }
}
