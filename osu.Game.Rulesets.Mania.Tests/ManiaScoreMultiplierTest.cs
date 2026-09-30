// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
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
            [new Mod[] { new ManiaModClassic() }, 1],
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

        private static readonly object[][] key_mod_multiplier_test_cases =
        [
            // score end date, client version, expected multiplier

            // scores verifiably from old clients.
            [new DateTimeOffset(2024, 1, 31, 11, 0, 0, TimeSpan.Zero), "2024.130.2", 1],
            [new DateTimeOffset(2024, 12, 9, 11, 0, 0, TimeSpan.Zero), "2024.1208.0", 1],
            [new DateTimeOffset(2025, 6, 12, 11, 0, 0, TimeSpan.Zero), "2025.605.3", 1],
            [new DateTimeOffset(2025, 6, 28, 11, 0, 0, TimeSpan.Zero), "2025.625.0-tachyon", 1],
            [new DateTimeOffset(2025, 7, 11, 11, 0, 0, TimeSpan.Zero), "2025.710.0-lazer", 1],
            [new DateTimeOffset(2025, 7, 15, 11, 0, 0, TimeSpan.Zero), "2025.711.0-tachyon", 1],

            // scores without explicit client versions, predating the change of multiplier.
            // those MUST have used the old multiplier.
            [new DateTimeOffset(2024, 1, 31, 11, 0, 0, TimeSpan.Zero), "", 1],
            [new DateTimeOffset(2024, 12, 9, 11, 0, 0, TimeSpan.Zero), "", 1],
            [new DateTimeOffset(2025, 6, 12, 11, 0, 0, TimeSpan.Zero), "", 1],
            [new DateTimeOffset(2025, 6, 28, 11, 0, 0, TimeSpan.Zero), "", 1],
            [new DateTimeOffset(2025, 7, 11, 11, 0, 0, TimeSpan.Zero), "", 1],
            [new DateTimeOffset(2025, 7, 15, 11, 0, 0, TimeSpan.Zero), "", 1],

            // scores without explicit client versions, AFTER the change of multiplier.
            // there is NO way of verifying whether these scores use the new or old multiplier, therefore GUESS that it's the new one.
            // "thankfully" the window of opportunity for this occurring *should* be slim
            // (from client release with new key mod multipliers on July 18, 2025
            // until spectator server release which added client version writing to server-side replays on August 1, 2025).
            [new DateTimeOffset(2025, 7, 19, 0, 20, 15, 0, TimeSpan.Zero), "", 0.9],
            [new DateTimeOffset(2025, 7, 23, 0, 20, 15, 0, TimeSpan.Zero), "", 0.9],
            [new DateTimeOffset(2025, 8, 19, 0, 20, 15, 0, TimeSpan.Zero), "", 0.9],
            [new DateTimeOffset(2026, 6, 18, 0, 20, 15, 0, TimeSpan.Zero), "", 0.9],
            [new DateTimeOffset(2026, 7, 18, 0, 20, 15, 0, TimeSpan.Zero), "", 0.9],

            // scores verifiably from new clients.
            [new DateTimeOffset(2025, 7, 19, 0, 20, 15, 0, TimeSpan.Zero), "2025.718.0-tachyon", 0.9],
            [new DateTimeOffset(2025, 7, 23, 0, 20, 15, 0, TimeSpan.Zero), "2025.721.0-tachyon", 0.9],
            [new DateTimeOffset(2025, 8, 19, 0, 20, 15, 0, TimeSpan.Zero), "2025.816.0-lazer", 0.9],
            [new DateTimeOffset(2026, 6, 18, 0, 20, 15, 0, TimeSpan.Zero), "2026.518.0-lazer", 0.9],
            [new DateTimeOffset(2026, 7, 18, 0, 20, 15, 0, TimeSpan.Zero), "2026.522.1-tachyon", 0.9],
        ];

        [TestCaseSource(nameof(key_mod_multiplier_test_cases))]
        public void TestKeyModMultiplierCompatibility(DateTimeOffset endDate, string clientVersion, double expectedMultiplier)
        {
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty(), new ScoreInfo
            {
                Date = endDate,
                ClientVersion = clientVersion
            }));
            Assert.That(calculator.CalculateFor([new ManiaModKey4()]), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }

        [TestCase(30000001, 0.96)]
        [TestCase(30000009, 0.96)]
        [TestCase(30000016, 0.96)]
        [TestCase(30000017, 1)]
        [TestCase(null, 1)]
        public void TestClassicMultiplierVersioning(int? totalScoreVersion, double expectedMultiplier)
        {
            var scoreInfo = totalScoreVersion != null ? new ScoreInfo { TotalScoreVersion = totalScoreVersion.Value } : null;
            var calculator = Ruleset.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty(), scoreInfo));
            Assert.That(calculator.CalculateFor([new ManiaModClassic()]), Is.EqualTo(expectedMultiplier).Within(Precision.DOUBLE_EPSILON));
        }
    }
}
