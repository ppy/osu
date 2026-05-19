// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.Rulesets.Scoring
{
    public class ScoreMultiplierCalculatorTest
    {
        [Test]
        public void TestFlatMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator();

            double multiplier = calculator.CalculateFor([new OsuModEasy()]);

            Assert.That(multiplier, Is.EqualTo(0.15));
        }

        [Test]
        public void TestSettingDependentMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator();

            double multiplier = calculator.CalculateFor([new OsuModDaycore { SpeedChange = { Value = 0.6 } }]);

            Assert.That(multiplier, Is.EqualTo(0.4));
        }

        [Test]
        public void TestContextDependentMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator();

            double multiplier;

            Assert.Multiple(() =>
            {
                calculator.HardRockPenalty = false;
                multiplier = calculator.CalculateFor([new OsuModHardRock()]);
                Assert.That(multiplier, Is.EqualTo(1.4));

                calculator.HardRockPenalty = true;
                multiplier = calculator.CalculateFor([new OsuModHardRock()]);
                Assert.That(multiplier, Is.EqualTo(1.2));
            });
        }

        [Test]
        public void TestCombinationMultiplier()
        {
            var calculator = new TestScoreMultiplierCalculator();

            double multiplier = calculator.CalculateFor([new OsuModEasy(), new OsuModDaycore()]);

            Assert.That(multiplier, Is.EqualTo(0.003));
        }

        [Test]
        public void TestCombinationAndFlatMultipliers()
        {
            var calculator = new TestScoreMultiplierCalculator();

            double multiplier = calculator.CalculateFor([new OsuModDaycore(), new OsuModHardRock(), new OsuModEasy()]);

            Assert.That(multiplier, Is.EqualTo(0.003 * 1.4));
        }

        private class TestScoreMultiplierCalculator : ScoreMultiplierCalculator
        {
            static TestScoreMultiplierCalculator()
            {
                Single<OsuModEasy>(hasMultiplier: 0.15);
                Single<OsuModDaycore>(hasMultiplier: daycore => (1 + daycore.SpeedChange.Value) / 4);
                Single<OsuModHardRock, TestScoreMultiplierCalculator>(hasMultiplier: (_, ctx) => ctx.HardRockPenalty ? 1.2 : 1.4);
                Combination<OsuModEasy, OsuModDaycore>(hasMultiplier: (_, _) => 0.003);
            }

            public bool HardRockPenalty { get; set; }
        }
    }
}
