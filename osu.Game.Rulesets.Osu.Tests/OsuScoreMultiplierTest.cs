// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Rulesets;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class OsuScoreMultiplierTest : RulesetScoreMultiplierTest
    {
        public OsuScoreMultiplierTest()
            : base(new OsuRuleset())
        {
        }

        private static readonly object[][] test_cases =
        [
            #region Difficulty Reduction

            [new Mod[] { new OsuModEasy() }, 0.5],
            [new Mod[] { new OsuModNoFail() }, 0.5],

            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.99 } } }, 0.5],

            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.50 } } }, 0.1],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.55 } } }, 0.1],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.60 } } }, 0.2],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.65 } } }, 0.2],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.70 } } }, 0.3],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.75 } } }, 0.3],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.80 } } }, 0.4],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.85 } } }, 0.4],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.90 } } }, 0.5],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.95 } } }, 0.5],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.99 } } }, 0.5],

            #endregion

            #region Difficulty Increase

            [new Mod[] { new OsuModHardRock() }, 1.06],
            [new Mod[] { new OsuModSuddenDeath() }, 1],
            [new Mod[] { new OsuModPerfect() }, 1],

            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.01 } } }, 1.00],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.05 } } }, 1.00],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.10 } } }, 1.02],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.15 } } }, 1.02],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.20 } } }, 1.04],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.25 } } }, 1.04],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.30 } } }, 1.06],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.35 } } }, 1.06],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.40 } } }, 1.08],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.45 } } }, 1.08],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.50 } } }, 1.10],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.55 } } }, 1.10],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.60 } } }, 1.12],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.65 } } }, 1.12],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.70 } } }, 1.14],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.75 } } }, 1.14],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.80 } } }, 1.16],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.85 } } }, 1.16],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.90 } } }, 1.18],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.95 } } }, 1.18],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 2.00 } } }, 1.20],

            [new Mod[] { new OsuModHidden() }, 1.06],
            [new Mod[] { new OsuModHidden { OnlyFadeApproachCircles = { Value = true } } }, 1],

            [new Mod[] { new OsuModTraceable() }, 1],

            [new Mod[] { new OsuModFlashlight() }, 1.12],
            [new Mod[] { new OsuModFlashlight { ComboBasedSize = { Value = false } } }, 1],

            [new Mod[] { new OsuModBlinds() }, 1.12],
            [new Mod[] { new OsuModStrictTracking() }, 1],
            [new Mod[] { new OsuModAccuracyChallenge() }, 1],

            #endregion

            #region Conversion

            [new Mod[] { new OsuModTargetPractice() }, 0.1],
            [new Mod[] { new OsuModDifficultyAdjust() }, 0.5],
            [new Mod[] { new OsuModClassic() }, 0.96],
            [new Mod[] { new OsuModRandom() }, 1],
            [new Mod[] { new OsuModMirror() }, 1],
            [new Mod[] { new OsuModAlternate() }, 1],
            [new Mod[] { new OsuModSingleTap() }, 1],

            #endregion

            #region Automation

            [new Mod[] { new OsuModAutoplay() }, 1],
            [new Mod[] { new OsuModCinema() }, 1],
            [new Mod[] { new OsuModRelax() }, 0.1],
            [new Mod[] { new OsuModAutopilot() }, 0.1],
            [new Mod[] { new OsuModSpunOut() }, 0.9],

            #endregion

            #region Fun

            [new Mod[] { new OsuModTransform() }, 1],
            [new Mod[] { new OsuModWiggle() }, 1],
            [new Mod[] { new OsuModSpinIn() }, 1],
            [new Mod[] { new OsuModGrow() }, 1],
            [new Mod[] { new OsuModDeflate() }, 1],
            [new Mod[] { new ModWindUp() }, 0.5],
            [new Mod[] { new ModWindDown() }, 0.5],
            [new Mod[] { new OsuModBarrelRoll() }, 1],
            [new Mod[] { new OsuModApproachDifferent() }, 1],
            [new Mod[] { new OsuModMuted() }, 1],
            [new Mod[] { new OsuModNoScope() }, 1],
            [new Mod[] { new OsuModMagnetised() }, 0.5],
            [new Mod[] { new OsuModRepel() }, 1],
            [new Mod[] { new ModAdaptiveSpeed() }, 0.5],
            [new Mod[] { new OsuModFreezeFrame() }, 1],
            [new Mod[] { new OsuModBubbles() }, 1],
            [new Mod[] { new OsuModSynesthesia() }, 0.8],
            [new Mod[] { new OsuModDepth() }, 1],
            [new Mod[] { new OsuModBloom() }, 1],

            #endregion

            #region System

            [new Mod[] { new OsuModTouchDevice() }, 1],
            [new Mod[] { new ModScoreV2() }, 1],

            #endregion

            #region Combinations

            [new Mod[] { new OsuModHidden(), new OsuModHardRock() }, 1.06 * 1.06],

            #endregion
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestMultipliers(Mod[] mods, double expectedMultiplier)
            => TestModCombination(mods, expectedMultiplier);
    }
}
