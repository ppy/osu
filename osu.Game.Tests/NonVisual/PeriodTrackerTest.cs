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
        private static readonly Period[] test_single_period = { new Period(1.0, 2.0) };

        // this is intended to be unordered to test adding periods in unordered way.
        private static readonly Period[] test_periods =
        {
            new Period(-9.1, -8.3),
            new Period(-3.4, 2.1),
            new Period(9.0, 50.0),
            new Period(5.25, 10.50)
        };

        [Test]
        public void TestCheckValueInsideSinglePeriod()
        {
            var tracker = new PeriodTracker { Periods = test_single_period };

            var period = test_single_period.Single();
            Assert.IsTrue(tracker.IsInAny(period.Start));
            Assert.IsTrue(tracker.IsInAny(getMidTime(period)));
            Assert.IsTrue(tracker.IsInAny(period.End));
        }

        [Test]
        public void TestCheckValuesInsidePeriods()
        {
            var tracker = new PeriodTracker { Periods = test_periods };

            foreach (var period in test_periods)
                Assert.IsTrue(tracker.IsInAny(getMidTime(period)));
        }

        [Test]
        public void TestCheckValuesInRandomOrder()
        {
            var tracker = new PeriodTracker { Periods = test_periods };

            foreach (var period in test_periods.OrderBy(_ => RNG.Next()))
                Assert.IsTrue(tracker.IsInAny(getMidTime(period)));
        }

        [Test]
        public void TestCheckValuesOutOfPeriods()
        {
            var tracker = new PeriodTracker
            {
                Periods = new[]
                {
                    new Period(1.0, 2.0),
                    new Period(3.0, 4.0)
                }
            };

            Assert.IsFalse(tracker.IsInAny(0.9), "Time before first period is being considered inside");

            Assert.IsFalse(tracker.IsInAny(2.1), "Time right after first period is being considered inside");
            Assert.IsFalse(tracker.IsInAny(2.9), "Time right before second period is being considered inside");

            Assert.IsFalse(tracker.IsInAny(4.1), "Time after last period is being considered inside");
        }

        [Test]
        public void TestNullRemovesExistingPeriods()
        {
            var tracker = new PeriodTracker { Periods = test_single_period };

            var period = test_single_period.Single();
            Assert.IsTrue(tracker.IsInAny(getMidTime(period)));

            tracker.Periods = null;
            Assert.IsFalse(tracker.IsInAny(getMidTime(period)));
        }

        [Test]
        public void TestReversedPeriodHandling()
        {
            var tracker = new PeriodTracker();

            Assert.Throws<ArgumentException>(() =>
            {
                tracker.Periods = new[]
                {
                    new Period(2.0, 1.0)
                };
            });
        }

        private double getMidTime(Period period) => period.Start + (period.End - period.Start) / 2;
    }
}
