// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class CatchScoreMultiplierTest : RulesetScoreMultiplierTest
    {
        public CatchScoreMultiplierTest()
            : base(new CatchRuleset())
        {
        }

        private static readonly object[][] test_cases =
        [
            #region Difficulty Reduction

            [new Mod[] { new CatchModEasy() }, 0.5],
            [new Mod[] { new CatchModNoFail() }, 0.5],

            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new CatchModHalfTime { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new CatchModDaycore { SpeedChange = { Value = 0.99 } } }, 0.5],

            #endregion

            #region Difficulty Increase

            [new Mod[] { new CatchModHardRock() }, 1.12],
            [new Mod[] { new CatchModSuddenDeath() }, 1],
            [new Mod[] { new CatchModPerfect() }, 1],

            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new CatchModDoubleTime { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new CatchModNightcore { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new CatchModHidden() }, 1.06],

            [new Mod[] { new CatchModFlashlight() }, 1.12],
            [new Mod[] { new CatchModFlashlight { ComboBasedSize = { Value = false } } }, 1],

            [new Mod[] { new ModAccuracyChallenge() }, 1],

            #endregion

            #region Conversion

            [new Mod[] { new CatchModDifficultyAdjust() }, 0.5],
            [new Mod[] { new CatchModClassic() }, 1],
            [new Mod[] { new CatchModMirror() }, 1],

            #endregion

            #region Automation

            [new Mod[] { new CatchModAutoplay() }, 1],
            [new Mod[] { new CatchModCinema() }, 1],
            [new Mod[] { new CatchModRelax() }, 0.1],

            #endregion

            #region Fun

            [new Mod[] { new ModWindUp() }, 0.5],
            [new Mod[] { new ModWindDown() }, 0.5],
            [new Mod[] { new CatchModFloatingFruits() }, 1],
            [new Mod[] { new CatchModMuted() }, 1],
            [new Mod[] { new CatchModNoScope() }, 1],
            [new Mod[] { new CatchModMovingFast() }, 1],
            [new Mod[] { new CatchModSynesthesia() }, 0.8],

            #endregion

            #region System

            [new Mod[] { new ModScoreV2() }, 1],

            #endregion

            #region Combinations

            [new Mod[] { new CatchModHidden(), new CatchModHardRock() }, 1.06 * 1.12]

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
            Assert.That(calculator.CalculateFor([new CatchModClassic()]), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }
    }
}
