// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuRateAdjustedDisplayDifficultyTest
    {
        private static IEnumerable<float> difficultyValuesToTest()
        {
            for (float i = 0; i <= 10; i += 0.5f)
                yield return i;
        }

        [TestCaseSource(nameof(difficultyValuesToTest))]
        public void TestApproachRateIsUnchangedWithRateEqualToOne(float originalApproachRate)
        {
            var ruleset = new OsuRuleset();
            var difficulty = new BeatmapDifficulty { ApproachRate = originalApproachRate };
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, []);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(originalApproachRate));
        }

        [TestCaseSource(nameof(difficultyValuesToTest))]
        public void TestOverallDifficultyIsUnchangedWithRateEqualToOne(float originalOverallDifficulty)
        {
            var ruleset = new OsuRuleset();
            var difficulty = new BeatmapDifficulty { OverallDifficulty = originalOverallDifficulty };
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, []);

            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(originalOverallDifficulty));
        }

        [Test]
        public void TestRateBelowOne()
        {
            var ruleset = new OsuRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new OsuModHalfTime()]);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(1.67).Within(0.01));
            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(2.22).Within(0.01));
        }

        [Test]
        public void TestRateAboveOne()
        {
            var ruleset = new OsuRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new OsuModDoubleTime()]);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(7.67).Within(0.01));
            Assert.That(adjustedDifficulty.OverallDifficulty, Is.EqualTo(7.77).Within(0.01));
        }
    }
}
