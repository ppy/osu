// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using FluentAssertions;
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
            algorithm.GetDisplayStartTime(2000, 0, 1000, 1).Should().Be(1000); // Like constant
            algorithm.GetDisplayStartTime(10500, 0, 1000, 1).Should().Be(10000); // 10500 - (1000 * 0.5)
            algorithm.GetDisplayStartTime(22000, 0, 1000, 1).Should().Be(20000); // 23000 - (1000 / 0.5)
        }

        [Test]
        public void TestObjectDisplayStartTime()
        {
            algorithm.GetDisplayStartTime(2000, 50, 1000, 500).Should().Be(900); // 2000 - (1 + 50 / 500) * 1000 / 1
            algorithm.GetDisplayStartTime(10000, 50, 1000, 500).Should().Be(9450); // 10000 - (1 + 50 / 500) * 1000 / 2
            algorithm.GetDisplayStartTime(15000, 250, 1000, 500).Should().Be(14250); // 15000 - (1 + 250 / 500) * 1000 / 2
            algorithm.GetDisplayStartTime(18000, 250, 2000, 500).Should().Be(16500); // 18000 - (1 + 250 / 500) * 2000 / 2
            algorithm.GetDisplayStartTime(20000, 50, 1000, 500).Should().Be(17800); // 20000 - (1 + 50 / 500) * 1000 / 0.5
            algorithm.GetDisplayStartTime(22000, 50, 1000, 500).Should().Be(19800); // 22000 - (1 + 50 / 500) * 1000 / 0.5
        }

        [Test]
        public void TestLength()
        {
            algorithm.GetLength(0, 1000, 5000, 1).Should().Be(1f / 5); // Like constant
            algorithm.GetLength(10000, 10500, 5000, 1).Should().Be(1f / 5); // (10500 - 10000) / 0.5 / 5000
            algorithm.GetLength(20000, 22000, 5000, 1).Should().Be(1f / 5); // (22000 - 20000) * 0.5 / 5000
        }

        [Test]
        public void TestPosition()
        {
            // Basically same calculations as TestLength()
            algorithm.PositionAt(1000, 0, 5000, 1).Should().Be(1f / 5);
            algorithm.PositionAt(10500, 10000, 5000, 1).Should().Be(1f / 5);
            algorithm.PositionAt(22000, 20000, 5000, 1).Should().Be(1f / 5);
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
            algorithm.TimeAt(algorithm.PositionAt(time, 0, 5000, 1), 0, 5000, 1).Should().BeApproximately(time, 0.001);
            algorithm.TimeAt(algorithm.PositionAt(time, 5000, 5000, 1), 5000, 5000, 1).Should().BeApproximately(time, 0.001);
        }
    }
}
