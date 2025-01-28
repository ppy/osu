// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoRateAdjustedDisplayDifficultyTest
    {
        private static IEnumerable<float> difficultyValuesToTest()
        {
            for (float i = 0; i <= 10; i += 0.5f)
                yield return i;
        }

        [TestCaseSource(nameof(difficultyValuesToTest))]
        public void TestOverallDifficultyIsUnchangedWithRateEqualToOne(float originalOverallDifficulty)
        {
            var ruleset = new TaikoRuleset();
            var difficulty = new BeatmapDifficulty { OverallDifficulty = originalOverallDifficulty };

            var adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(difficulty, 1);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(originalOverallDifficulty));
        }

        [Test]
        public void TestRateBelowOne()
        {
            var ruleset = new TaikoRuleset();
            var difficulty = new BeatmapDifficulty();

            var adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(difficulty, 0.75);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(1.11).Within(0.01));
        }

        [Test]
        public void TestRateAboveOne()
        {
            var ruleset = new TaikoRuleset();
            var difficulty = new BeatmapDifficulty();

            var adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(difficulty, 1.5);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(8.89).Within(0.01));
        }
    }
}
