// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
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

            [new Mod[] { new OsuModEasy() }, 0.8],
            [new Mod[] { new OsuModEasy { Retries = { Value = 1 } } }, 0.8],
            [new Mod[] { new OsuModEasy { Retries = { Value = 3 } } }, 0.7],
            [new Mod[] { new OsuModEasy { Retries = { Value = 5 } } }, 0.5],
            [new Mod[] { new OsuModEasy { Retries = { Value = 8 } } }, 0.4],

            [new Mod[] { new OsuModNoFail() }, 0.5],

            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.50 } } }, 0.20],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.55 } } }, 0.27],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.60 } } }, 0.34],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.65 } } }, 0.41],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.70 } } }, 0.48],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.75 } } }, 0.55],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.80 } } }, 0.62],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.85 } } }, 0.69],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.90 } } }, 0.76],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.95 } } }, 0.83],
            [new Mod[] { new OsuModHalfTime { SpeedChange = { Value = 0.99 } } }, 0.83],

            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.50 } } }, 0.20],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.55 } } }, 0.27],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.60 } } }, 0.34],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.65 } } }, 0.41],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.70 } } }, 0.48],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.75 } } }, 0.55],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.80 } } }, 0.62],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.85 } } }, 0.69],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.90 } } }, 0.76],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.95 } } }, 0.83],
            [new Mod[] { new OsuModDaycore { SpeedChange = { Value = 0.99 } } }, 0.83],

            #endregion

            #region Difficulty Increase

            [new Mod[] { new OsuModHardRock() }, 1.09],
            [new Mod[] { new OsuModSuddenDeath() }, 1],
            [new Mod[] { new OsuModPerfect() }, 1],

            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } }, 1.000],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.05 } } }, 1.000],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.10 } } }, 1.036],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.15 } } }, 1.036],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.20 } } }, 1.082],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.25 } } }, 1.082],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.30 } } }, 1.128],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.35 } } }, 1.128],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.40 } } }, 1.174],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.45 } } }, 1.174],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.50 } } }, 1.230],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.55 } } }, 1.230],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.60 } } }, 1.266],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.65 } } }, 1.266],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.70 } } }, 1.312],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.75 } } }, 1.312],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.80 } } }, 1.358],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.85 } } }, 1.358],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.90 } } }, 1.404],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 1.95 } } }, 1.404],
            [new Mod[] { new OsuModDoubleTime { SpeedChange = { Value = 2.00 } } }, 1.450],

            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.01 } } }, 1.000],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.05 } } }, 1.000],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.10 } } }, 1.036],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.15 } } }, 1.036],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.20 } } }, 1.082],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.25 } } }, 1.082],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.30 } } }, 1.128],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.35 } } }, 1.128],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.40 } } }, 1.174],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.45 } } }, 1.174],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.50 } } }, 1.230],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.55 } } }, 1.230],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.60 } } }, 1.266],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.65 } } }, 1.266],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.70 } } }, 1.312],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.75 } } }, 1.312],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.80 } } }, 1.358],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.85 } } }, 1.358],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.90 } } }, 1.404],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 1.95 } } }, 1.404],
            [new Mod[] { new OsuModNightcore { SpeedChange = { Value = 2.00 } } }, 1.450],

            [new Mod[] { new OsuModHidden() }, 1.04],
            [new Mod[] { new OsuModHidden { OnlyFadeApproachCircles = { Value = true } } }, 1.02],

            [new Mod[] { new OsuModTraceable() }, 1.02],

            [new Mod[] { new OsuModFlashlight() }, 1.2],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 0.5f } } }, 1.2],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 0.9f } } }, 1.2],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 1.1f } } }, 1.18],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 1.5f } } }, 1.1],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 1.9f } } }, 1.02],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 2f } } }, 1.02],
            [new Mod[] { new OsuModFlashlight { ComboBasedSize = { Value = false } } }, 1.04],
            [new Mod[] { new OsuModFlashlight { SizeMultiplier = { Value = 1.9f }, ComboBasedSize = { Value = false } } }, 1.004],

            [new Mod[] { new OsuModBlinds() }, 1.24],
            [new Mod[] { new OsuModStrictTracking() }, 1],
            [new Mod[] { new OsuModAccuracyChallenge() }, 1],

            #endregion

            #region Conversion

            [new Mod[] { new OsuModTargetPractice() }, 0.01],
            [new Mod[] { new OsuModDifficultyAdjust() }, 1],

            [new Mod[] { new OsuModClassic() }, 0.985],
            [new Mod[] { new OsuModClassic { ClassicNoteLock = { Value = false } } }, 0.96],

            [new Mod[] { new OsuModRandom() }, 0.7],
            [new Mod[] { new OsuModMirror() }, 1],
            [new Mod[] { new OsuModAlternate() }, 1],
            [new Mod[] { new OsuModSingleTap() }, 1],

            #endregion

            #region Automation

            [new Mod[] { new OsuModAutoplay() }, 1],
            [new Mod[] { new OsuModCinema() }, 1],
            [new Mod[] { new OsuModRelax() }, 0.1],
            [new Mod[] { new OsuModAutopilot() }, 0.1],
            [new Mod[] { new OsuModSpunOut() }, 0.95],

            #endregion

            #region Fun

            [new Mod[] { new OsuModTransform() }, 1],
            [new Mod[] { new OsuModWiggle() }, 1],
            [new Mod[] { new OsuModSpinIn() }, 1],
            [new Mod[] { new OsuModGrow() }, 1],

            [new Mod[] { new OsuModDeflate() }, 1],
            [new Mod[] { new OsuModDeflate { StartScale = { Value = 5 } } }, 0.94],

            [new Mod[] { new ModWindUp() }, 0.8 * 1 + 0.2 * 1.230],
            [new Mod[] { new ModWindUp { InitialRate = { Value = 0.7 }, FinalRate = { Value = 1.2 } } }, 0.8 * 0.48 + 0.2 * 1.082],
            [new Mod[] { new ModWindUp { InitialRate = { Value = 0.7 }, FinalRate = { Value = 0.9 } } }, 0.8 * 0.48 + 0.2 * 0.76],
            [new Mod[] { new ModWindUp { InitialRate = { Value = 1.1 }, FinalRate = { Value = 1.4 } } }, 0.8 * 1.036 + 0.2 * 1.174],

            [new Mod[] { new ModWindDown() }, 0.8 * 0.55 + 0.2 * 1],
            [new Mod[] { new ModWindDown { InitialRate = { Value = 1.2 }, FinalRate = { Value = 0.7 } } }, 0.8 * 0.48 + 0.2 * 1.082],
            [new Mod[] { new ModWindDown { InitialRate = { Value = 0.9 }, FinalRate = { Value = 0.7 } } }, 0.8 * 0.48 + 0.2 * 0.76],
            [new Mod[] { new ModWindDown { InitialRate = { Value = 1.4 }, FinalRate = { Value = 1.1 } } }, 0.8 * 1.036 + 0.2 * 1.174],

            [new Mod[] { new OsuModBarrelRoll() }, 1],
            [new Mod[] { new OsuModApproachDifferent() }, 0.7],
            [new Mod[] { new OsuModMuted() }, 1],
            [new Mod[] { new OsuModNoScope() }, 1],

            [new Mod[] { new OsuModMagnetised() }, 0.4],
            [new Mod[] { new OsuModMagnetised { AttractionStrength = { Value = 0.05f } } }, 0.67],
            [new Mod[] { new OsuModMagnetised { AttractionStrength = { Value = 0.2f } } }, 0.58],
            [new Mod[] { new OsuModMagnetised { AttractionStrength = { Value = 0.7f } } }, 0.28],
            [new Mod[] { new OsuModMagnetised { AttractionStrength = { Value = 1 } } }, 0.1],

            [new Mod[] { new OsuModRepel() }, 1],
            [new Mod[] { new ModAdaptiveSpeed() }, 0.1],
            [new Mod[] { new OsuModFreezeFrame() }, 1],
            [new Mod[] { new OsuModBubbles() }, 1],
            [new Mod[] { new OsuModSynesthesia() }, 0.99],
            [new Mod[] { new OsuModDepth() }, 1],
            [new Mod[] { new OsuModBloom() }, 1],

            #endregion

            #region System

            [new Mod[] { new OsuModTouchDevice() }, 1],
            [new Mod[] { new ModScoreV2() }, 1],

            #endregion

            #region Combinations

            [new Mod[] { new OsuModHidden(), new OsuModHardRock() }, 1.04 * 1.09],

            [new Mod[] { new OsuModHidden(), new OsuModWiggle() }, 1.02],
            [new Mod[] { new OsuModHidden(), new OsuModGrow() }, 1.02],
            [new Mod[] { new OsuModHidden(), new OsuModDeflate() }, 1.02],
            [new Mod[] { new OsuModHidden(), new OsuModDeflate { StartScale = { Value = 4 } } }, 1.02 * 0.96],
            [new Mod[] { new OsuModHidden(), new OsuModRepel() }, 1.02],
            [new Mod[] { new OsuModHidden { OnlyFadeApproachCircles = { Value = true } }, new OsuModRepel() }, 1],
            [new Mod[] { new OsuModHidden(), new OsuModDepth() }, 1.02],
            [new Mod[] { new OsuModHidden(), new OsuModDepth(), new OsuModHardRock() }, 1.02 * 1.09],

            [new Mod[] { new OsuModHidden(), new OsuModBlinds() }, 1.24],
            [new Mod[] { new OsuModHidden(), new OsuModBlinds(), new OsuModHardRock() }, 1.24 * 1.09],

            [new Mod[] { new OsuModTraceable(), new OsuModBlinds() }, 1.24],
            [new Mod[] { new OsuModTraceable(), new OsuModBlinds(), new OsuModHardRock() }, 1.24 * 1.09],

            [new Mod[] { new OsuModFlashlight(), new OsuModFreezeFrame() }, 1.1],

            #endregion
        ];

        [TestCaseSource(nameof(test_cases))]
        public void TestMultipliers(Mod[] mods, double expectedMultiplier)
            => TestModCombination(mods, expectedMultiplier);

        [TestCase(null, null, null, null, 1)]
        [TestCase(2.9f, null, null, null, 0.95)]
        [TestCase(3.1f, null, null, null, 0.95)]
        [TestCase(null, 3.9f, null, null, 0.95)]
        [TestCase(null, 4.1f, null, null, 0.95)]
        [TestCase(null, null, 4.9f, null, 0.95)]
        [TestCase(null, null, 5.1f, null, 0.95)]
        [TestCase(null, null, null, 5.9f, 0.95)]
        [TestCase(null, null, null, 6.1f, 0.95)]
        [TestCase(2.9f, 3.9f, null, null, 0.95 * 0.95)]
        [TestCase(2.9f, 3.9f, 4.9f, null, 0.95 * 0.95 * 0.95)]
        [TestCase(2.9f, 3.9f, 4.9f, 5.9f, 0.95 * 0.95 * 0.95 * 0.95)]
        [TestCase(0.0f, null, null, null, 0.1)]
        [TestCase(0.0f, 0.0f, 0.0f, 0.0f, 0.1)]
        public void TestDifficultyAdjust(float? cs, float? ar, float? od, float? hp, double expectedMultiplier)
        {
            var difficulty = new BeatmapDifficulty
            {
                CircleSize = 3,
                ApproachRate = 4,
                OverallDifficulty = 5,
                DrainRate = 6,
            };
            var mod = new OsuModDifficultyAdjust
            {
                CircleSize = { Value = cs },
                ApproachRate = { Value = ar },
                OverallDifficulty = { Value = od },
                DrainRate = { Value = hp },
            };

            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(difficulty));
            Assert.That(calculator.CalculateFor([mod]), Is.EqualTo(expectedMultiplier).Within(Precision.FLOAT_EPSILON));
        }

        [TestCase(30000001, 0.96)]
        [TestCase(30000009, 0.96)]
        [TestCase(30000016, 0.96)]
        [TestCase(30000017, 0.985)]
        [TestCase(null, 0.985)]
        public void TestClassicMultiplierVersioning(int? totalScoreVersion, double expectedMultiplier)
        {
            var scoreInfo = totalScoreVersion != null ? new ScoreInfo { TotalScoreVersion = totalScoreVersion.Value } : null;
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty(), scoreInfo));
            Assert.That(calculator.CalculateFor([new OsuModClassic()]), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }

        [Test]
        public void VerySmallModMultiplier()
        {
            var mods = new Mod[]
            {
                new OsuModEasy { Retries = { Value = 10 } },
                new OsuModNoFail(),
                new OsuModHalfTime { SpeedChange = { Value = 0.5 } },
                new OsuModTargetPractice(),
                new OsuModClassic { ClassicNoteLock = { Value = false } },
                new OsuModDeflate { StartScale = { Value = 25 } },
                new OsuModMagnetised { AttractionStrength = { Value = 1 } },
                new OsuModSynesthesia(),
            };
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));
            Assert.That(calculator.CalculateFor(mods), Is.GreaterThan(0));
        }
    }
}
