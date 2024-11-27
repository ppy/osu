// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class PeriodTrackerTest
    {
        private static readonly Period[] single_period = { new Period(1.0, 2.0) };

        private static readonly Period[] unordered_periods =
        {
            new Period(-9.1, -8.3),
            new Period(-3.4, 2.1),
            new Period(9.0, 50.0),
            new Period(5.25, 10.50)
        };

        [Test]
        public void TestCheckValueInsideSinglePeriod()
        {
            var tracker = new PeriodTracker(single_period);

            var period = single_period.Single();
            Assert.IsTrue(tracker.IsInAny(period.Start));
            Assert.IsTrue(tracker.IsInAny(getMidpoint(period)));
            Assert.IsTrue(tracker.IsInAny(period.End));
        }

        [Test]
        public void TestCheckValuesInsidePeriods()
        {
            var tracker = new PeriodTracker(unordered_periods);

            foreach (var period in unordered_periods)
                Assert.IsTrue(tracker.IsInAny(getMidpoint(period)));
        }

        [Test]
        public void TestCheckValuesInRandomOrder()
        {
            var tracker = new PeriodTracker(unordered_periods);

            foreach (var period in unordered_periods.OrderBy(_ => RNG.Next()))
                Assert.IsTrue(tracker.IsInAny(getMidpoint(period)));
        }

        [Test]
        public void TestCheckValuesOutOfPeriods()
        {
            var tracker = new PeriodTracker(new[]
            {
                new Period(1.0, 2.0),
                new Period(3.0, 4.0)
            });

            Assert.IsFalse(tracker.IsInAny(0.9), "Time before first period is being considered inside");

            Assert.IsFalse(tracker.IsInAny(2.1), "Time right after first period is being considered inside");
            Assert.IsFalse(tracker.IsInAny(2.9), "Time right before second period is being considered inside");

            Assert.IsFalse(tracker.IsInAny(4.1), "Time after last period is being considered inside");
        }

        [Test]
        public void TestReversedPeriodHandling()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new PeriodTracker(new[]
                {
                    new Period(2.0, 1.0)
                });
            });
        }

        private double getMidpoint(Period period) => period.Start + (period.End - period.Start) / 2;
    }
}
