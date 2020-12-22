// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Lists;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Tests.ScrollAlgorithms
{
    [TestFixture]
    public class OverlappingScrollTest
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

            algorithm = new OverlappingScrollAlgorithm(controlPoints);
        }

        [Test]
        public void TestPointDisplayStartTime()
        {
            Assert.AreEqual(1000, algorithm.GetDisplayStartTime(2000, 0, 1000, 1)); // Like constant
            Assert.AreEqual(10000, algorithm.GetDisplayStartTime(10500, 0, 1000, 1)); // 10500 - (1000 * 0.5)
            Assert.AreEqual(20000, algorithm.GetDisplayStartTime(22000, 0, 1000, 1)); // 23000 - (1000 / 0.5)
        }

        [Test]
        public void TestObjectDisplayStartTime()
        {
            Assert.AreEqual(900, algorithm.GetDisplayStartTime(2000, 50, 1000, 500)); // 2000 - (1 + 50 / 500) * 1000 / 1
            Assert.AreEqual(9450, algorithm.GetDisplayStartTime(10000, 50, 1000, 500)); // 10000 - (1 + 50 / 500) * 1000 / 2
            Assert.AreEqual(14250, algorithm.GetDisplayStartTime(15000, 250, 1000, 500)); // 15000 - (1 + 250 / 500) * 1000 / 2
            Assert.AreEqual(16500, algorithm.GetDisplayStartTime(18000, 250, 2000, 500)); // 18000 - (1 + 250 / 500) * 2000 / 2
            Assert.AreEqual(17800, algorithm.GetDisplayStartTime(20000, 50, 1000, 500)); // 20000 - (1 + 50 / 500) * 1000 / 0.5
            Assert.AreEqual(19800, algorithm.GetDisplayStartTime(22000, 50, 1000, 500)); // 22000 - (1 + 50 / 500) * 1000 / 0.5
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
        [Ignore("Disabled for now because overlapping control points have multiple time values under the same position."
                + "Ideally, scrolling should be changed to constant or sequential during editing of hitobjects.")]
        public void TestTime(double time)
        {
            Assert.AreEqual(time, algorithm.TimeAt(algorithm.PositionAt(time, 0, 5000, 1), 0, 5000, 1), 0.001);
            Assert.AreEqual(time, algorithm.TimeAt(algorithm.PositionAt(time, 5000, 5000, 1), 5000, 5000, 1), 0.001);
        }
    }
}
