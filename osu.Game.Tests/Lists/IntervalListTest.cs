// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Lists;

namespace osu.Game.Tests.Lists
{
    [TestFixture]
    public class IntervalListTest
    {
        // this is intended to be unordered to test adding intervals in unordered way.
        private static readonly (double, double)[] test_intervals =
        {
            (-9.1d, -8.3d),
            (-3.4d, 2.1d),
            (9.0d, 50.0d),
            (5.25d, 10.50d),
        };

        [Test]
        public void TestCheckValueInsideSingleInterval()
        {
            var list = new IntervalList<double> { { 1.0d, 2.0d } };

            Assert.IsTrue(list.IsInAnyInterval(1.0d));
            Assert.IsTrue(list.IsInAnyInterval(1.5d));
            Assert.IsTrue(list.IsInAnyInterval(2.0d));
        }

        [Test]
        public void TestCheckValuesInsideIntervals()
        {
            var list = new IntervalList<double>();

            foreach (var (start, end) in test_intervals)
                list.Add(start, end);

            Assert.IsTrue(list.IsInAnyInterval(-8.75d));
            Assert.IsTrue(list.IsInAnyInterval(1.0d));
            Assert.IsTrue(list.IsInAnyInterval(7.89d));
            Assert.IsTrue(list.IsInAnyInterval(9.8d));
            Assert.IsTrue(list.IsInAnyInterval(15.83d));
        }

        [Test]
        public void TestCheckValuesInRandomOrder()
        {
            var list = new IntervalList<double>();

            foreach (var (start, end) in test_intervals)
                list.Add(start, end);

            Assert.IsTrue(list.IsInAnyInterval(9.8d));
            Assert.IsTrue(list.IsInAnyInterval(7.89d));
            Assert.IsTrue(list.IsInAnyInterval(1.0d));
            Assert.IsTrue(list.IsInAnyInterval(15.83d));
            Assert.IsTrue(list.IsInAnyInterval(-8.75d));
        }

        [Test]
        public void TestCheckValuesOutOfIntervals()
        {
            var list = new IntervalList<double>();

            foreach (var (start, end) in test_intervals)
                list.Add(start, end);

            Assert.IsFalse(list.IsInAnyInterval(-9.2d));
            Assert.IsFalse(list.IsInAnyInterval(2.2d));
            Assert.IsFalse(list.IsInAnyInterval(5.15d));
            Assert.IsFalse(list.IsInAnyInterval(51.2d));
        }

        [Test]
        public void TestCheckValueAfterRemovedInterval()
        {
            var list = new IntervalList<int> { { 50, 100 }, { 150, 200 }, { 250, 300 } };

            Assert.IsTrue(list.IsInAnyInterval(75));
            Assert.IsTrue(list.IsInAnyInterval(175));
            Assert.IsTrue(list.IsInAnyInterval(275));

            list.Remove(list[1]);

            Assert.IsFalse(list.IsInAnyInterval(175));
            Assert.IsTrue(list.IsInAnyInterval(75));
            Assert.IsTrue(list.IsInAnyInterval(275));
        }

        [Test]
        public void TestCheckValueAfterAddedInterval()
        {
            var list = new IntervalList<int> { { 50, 100 }, { 250, 300 } };

            Assert.IsFalse(list.IsInAnyInterval(175));
            Assert.IsTrue(list.IsInAnyInterval(75));
            Assert.IsTrue(list.IsInAnyInterval(275));

            list.Add(150, 200);

            Assert.IsTrue(list.IsInAnyInterval(175));
        }

        [Test]
        public void TestReversedIntervalThrows()
        {
            var list = new IntervalList<int>();

            Assert.Throws<ArgumentException>(() => list.Add(50, 25));
            Assert.Throws<ArgumentException>(() => list.Add(new Interval<int>(50, 25)));
        }
    }
}
