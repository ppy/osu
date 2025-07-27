// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaScoreProcessorTest
    {
        [TestCase(ScoreRank.X, 1, HitResult.Perfect)]
        [TestCase(ScoreRank.X, 0.99, HitResult.Great)]
        [TestCase(ScoreRank.D, 0.1, HitResult.Great)]
        [TestCase(ScoreRank.X, 0.99, HitResult.Perfect, HitResult.Great)]
        [TestCase(ScoreRank.X, 0.99, HitResult.Great, HitResult.Great)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Perfect, HitResult.Good)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Perfect, HitResult.Ok)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Perfect, HitResult.Meh)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Perfect, HitResult.Miss)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Great, HitResult.Good)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Great, HitResult.Ok)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Great, HitResult.Meh)]
        [TestCase(ScoreRank.S, 0.99, HitResult.Great, HitResult.Miss)]
        public void TestRanks(ScoreRank expected, double accuracy, params HitResult[] results)
        {
            var scoreProcessor = new ManiaScoreProcessor();

            Dictionary<HitResult, int> resultsDict = new Dictionary<HitResult, int>();
            foreach (var result in results)
                resultsDict[result] = resultsDict.GetValueOrDefault(result) + 1;

            Assert.That(scoreProcessor.RankFromScore(accuracy, resultsDict), Is.EqualTo(expected));
        }
    }
}
