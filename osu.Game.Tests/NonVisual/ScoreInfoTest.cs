// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class ScoreInfoTest
    {
        [Test]
        public void TestDeepClone()
        {
            var score = new ScoreInfo();

            score.Statistics.Add(HitResult.Good, 10);
            score.Rank = ScoreRank.B;

            var scoreCopy = score.DeepClone();

            score.Statistics[HitResult.Good]++;
            score.Rank = ScoreRank.X;

            Assert.That(scoreCopy.Statistics[HitResult.Good], Is.EqualTo(10));
            Assert.That(score.Statistics[HitResult.Good], Is.EqualTo(11));

            Assert.That(scoreCopy.Rank, Is.EqualTo(ScoreRank.B));
            Assert.That(score.Rank, Is.EqualTo(ScoreRank.X));
        }
    }
}
