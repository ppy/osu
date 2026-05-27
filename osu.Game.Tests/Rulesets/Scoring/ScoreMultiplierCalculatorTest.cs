// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Tests.Rulesets.Scoring
{
    public class ScoreMultiplierCalculatorTest
    {
        [Test]
        public void TestFlatMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));

            double multiplier = calculator.CalculateFor([new OsuModEasy()]);

            Assert.That(multiplier, Is.EqualTo(0.15));
        }

        [Test]
        public void TestSettingDependentMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));

            double multiplier = calculator.CalculateFor([new OsuModDaycore { SpeedChange = { Value = 0.6 } }]);

            Assert.That(multiplier, Is.EqualTo(0.4));
        }

        [Test]
        public void TestScoreDependentMultiplier()
        {
            TestScoreMultiplierCalculator calculator;

            double multiplier;

            Assert.Multiple(() =>
            {
                calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));
                multiplier = calculator.CalculateFor([new OsuModHardRock()]);
                Assert.That(multiplier, Is.EqualTo(1.4));

                calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty(), new ScoreInfo { ClientVersion = "2024.123.0" }));
                multiplier = calculator.CalculateFor([new OsuModHardRock()]);
                Assert.That(multiplier, Is.EqualTo(1.2));
            });
        }

        [Test]
        public void TestDifficultyDependentMultiplier()
        {
            TestScoreMultiplierCalculator calculator;

            double multiplier;

            Assert.Multiple(() =>
            {
                calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));
                multiplier = calculator.CalculateFor([new OsuModEasy()]);
                Assert.That(multiplier, Is.EqualTo(0.15));

                calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty { ApproachRate = 0 }));
                multiplier = calculator.CalculateFor([new OsuModEasy()]);
                Assert.That(multiplier, Is.EqualTo(0.1));
            });
        }

        [Test]
        public void TestCombinationMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));

            double multiplier = calculator.CalculateFor([new OsuModEasy(), new OsuModDaycore()]);

            Assert.That(multiplier, Is.EqualTo(0.003));
        }

        [Test]
        public void TestCombinationAndFlatMultipliers()
        {
            var calculator = new TestScoreMultiplierCalculator(new ScoreMultiplierContext(new BeatmapDifficulty()));

            double multiplier = calculator.CalculateFor([new OsuModDaycore(), new OsuModHardRock(), new OsuModEasy()]);

            Assert.That(multiplier, Is.EqualTo(0.003 * 1.4));
        }

        private class TestScoreMultiplierCalculator : ScoreMultiplierCalculator
        {
            public TestScoreMultiplierCalculator(ScoreMultiplierContext context)
                : base(context)
            {
                Single<OsuModEasy>(hasMultiplier: context.BeatmapDifficultyWithoutMods.ApproachRate == 0 ? 0.1 : 0.15);
                Single<OsuModDaycore>(hasMultiplier: daycore => (1 + daycore.SpeedChange.Value) / 4);
                Single<OsuModHardRock>(hasMultiplier: _ => context.Score?.ClientVersion == "2024.123.0" ? 1.2 : 1.4);
                Combination<OsuModEasy, OsuModDaycore>(hasMultiplier: (_, _) => 0.003);
            }
        }
    }
}
