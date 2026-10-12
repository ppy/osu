// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchRateAdjustedDisplayDifficultyTest
    {
        private static IEnumerable<float> difficultyValuesToTest()
        {
            for (float i = 0; i <= 10; i += 0.5f)
                yield return i;
        }

        [TestCaseSource(nameof(difficultyValuesToTest))]
        public void TestApproachRateIsUnchangedWithRateEqualToOne(float originalApproachRate)
        {
            var ruleset = new CatchRuleset();
            var difficulty = new BeatmapDifficulty { ApproachRate = originalApproachRate };
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, []);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(originalApproachRate));
        }

        [Test]
        public void TestRateBelowOne()
        {
            var ruleset = new CatchRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new CatchModHalfTime()]);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(1.67).Within(0.01));
        }

        [Test]
        public void TestRateAboveOne()
        {
            var ruleset = new CatchRuleset();
            var difficulty = new BeatmapDifficulty();
            var beatmapInfo = new BeatmapInfo { Difficulty = difficulty };

            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(beatmapInfo, [new CatchModDoubleTime()]);

            Assert.That(adjustedDifficulty.ApproachRate, Is.EqualTo(7.67).Within(0.01));
        }
    }
}
