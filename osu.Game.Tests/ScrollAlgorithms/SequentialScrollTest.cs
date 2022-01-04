// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Lists;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Tests.ScrollAlgorithms
{
    [TestFixture]
    public class SequentialScrollTest
    {
        private IScrollAlgorithm algorithm;

        [SetUp]
        public void Setup()
        {
            var controlPoints = new SortedList<MultiplierControlPoint>
            {
                new MultiplierControlPoint(0) { Velocity = 1 },
                new MultiplierControlPoint(10000) { Velocity = 2f },
                new MultiplierControlPoint(20000) { Velocity = 0.5f }
            };

            algorithm = new SequentialScrollAlgorithm(controlPoints);
        }

        [Test]
        public void TestDisplayStartTime()
        {
            // easy cases - time range adjusted for velocity fits within control point duration
            Assert.AreEqual(2500, algorithm.GetDisplayStartTime(5000, 0, 2500, 1)); // 5000 - (2500 / 1)
            Assert.AreEqual(13750, algorithm.GetDisplayStartTime(15000, 0, 2500, 1)); // 15000 - (2500 / 2)
            Assert.AreEqual(20000, algorithm.GetDisplayStartTime(25000, 0, 2500, 1)); // 25000 - (2500 / 0.5)

            // hard case - time range adjusted for velocity exceeds control point duration

            // 1st multiplier point takes 10000 / 2500 = 4 scroll lengths
            // 2nd multiplier point takes 10000 / (2500 / 2) = 8 scroll lengths
            // 3rd multiplier point takes 2500 / (2500 * 2) = 0.5 scroll lengths up to hitobject start

            // absolute position of the hitobject = 1000 * (4 + 8 + 0.5) = 12500
            // minus one scroll length allowance = 12500 - 1000 = 11500 = 11.5 [scroll lengths]
            // therefore the start time lies within the second multiplier point (because 11.5 < 4 + 8)
            // its exact time position is = 10000 + 7.5 * (2500 / 2) = 19375
            Assert.AreEqual(19375, algorithm.GetDisplayStartTime(22500, 0, 2500, 1000));
        }

        [Test]
        public void TestLength()
        {
            Assert.AreEqual(1f / 5, algorithm.GetLength(0, 1000, 5000, 1)); // Like constant
            Assert.AreEqual(1f / 5, algorithm.GetLength(10000, 10500, 5000, 1)); // (10500 - 10000) / 0.5 / 5000
            Assert.AreEqual(1f / 5, algorithm.GetLength(20000, 22000, 5000, 1)); // (22000 - 20000) * 0.5 / 5000
        }

        [Test]
        public void TestPosition()
        {
            // Basically same calculations as TestLength()
            Assert.AreEqual(1f / 5, algorithm.PositionAt(1000, 0, 5000, 1));
            Assert.AreEqual(1f / 5, algorithm.PositionAt(10500, 10000, 5000, 1));
            Assert.AreEqual(1f / 5, algorithm.PositionAt(22000, 20000, 5000, 1));
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(15000)]
        [TestCase(20000)]
        [TestCase(25000)]
        public void TestTime(double time)
        {
            Assert.AreEqual(time, algorithm.TimeAt(algorithm.PositionAt(time, 0, 5000, 1), 0, 5000, 1), 0.001);
            Assert.AreEqual(time, algorithm.TimeAt(algorithm.PositionAt(time, 5000, 5000, 1), 5000, 5000, 1), 0.001);
        }
    }
}
