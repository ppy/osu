// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Legacy;

namespace osu.Game.Tests.Rulesets.Scoring
{
    public class DifficultyPeppyStarsTest
    {
        // https://osu.ppy.sh/beatmapsets/1156087#osu/2625853
        [TestCase(3f, 3.8f, 4f, 158, 80, ExpectedResult = 4)]
        // https://osu.ppy.sh/beatmapsets/2109584#osu/4455731
        [TestCase(3.4f, 4f, 4f, 152, 80, ExpectedResult = 3)]
        // https://osu.ppy.sh/beatmapsets/286240#osu/657497
        [TestCase(3.7f, 5f, 5f, 129, 80, ExpectedResult = 3)]
        // https://osu.ppy.sh/beatmapsets/1654912#osu/3379490
        [TestCase(3.4f, 4f, 4f, 285, 150, ExpectedResult = 3)]
        // https://osu.ppy.sh/beatmapsets/314211#osu/712859
        [TestCase(3.4f, 4f, 4f, 95, 50, ExpectedResult = 3)]
        public int TestDifficultyPeppyStars(float circleSize, float drainRate, float overallDifficulty, int objectCount, int drainLength)
            => LegacyRulesetExtensions.CalculateDifficultyPeppyStars(new BeatmapDifficulty
            {
                CircleSize = circleSize,
                DrainRate = drainRate,
                OverallDifficulty = overallDifficulty,
            }, objectCount, drainLength);
    }
}
