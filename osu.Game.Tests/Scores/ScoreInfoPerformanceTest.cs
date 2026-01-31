// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;

namespace osu.Game.Tests.Scores
{
    [TestFixture]
    public class ScoreInfoPerformanceTest
    {
        [Test]
        public void TestGetStatisticsForDisplayPerformance()
        {
            var ruleset = new OsuRuleset();
            var score = new ScoreInfo(ruleset: ruleset.RulesetInfo);

            // Populate statistics with some dummy data
            foreach (var result in ruleset.GetHitResults())
            {
                score.Statistics[result.result] = 100;
            }

            // Warmup
            for (int i = 0; i < 100; i++)
            {
                var stats = score.GetStatisticsForDisplay().ToList();
            }

            // Benchmark
            int iterations = 10000;
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                var stats = score.GetStatisticsForDisplay().ToList();
            }

            stopwatch.Stop();
            Console.WriteLine($"GetStatisticsForDisplay executed {iterations} times in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
