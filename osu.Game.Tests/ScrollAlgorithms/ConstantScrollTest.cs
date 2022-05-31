// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using FluentAssertions;
using NUnit.Framework;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Tests.ScrollAlgorithms
{
    [TestFixture]
    public class ConstantScrollTest
    {
        private IScrollAlgorithm algorithm;

        [SetUp]
        public void Setup()
        {
            algorithm = new ConstantScrollAlgorithm();
        }

        [Test]
        public void TestPointDisplayStartTime()
        {
            algorithm.GetDisplayStartTime(2000, 0, 10000, 1).Should().Be(-8000);
            algorithm.GetDisplayStartTime(2000, 0, 5000, 1).Should().Be(-3000);
            algorithm.GetDisplayStartTime(7000, 0, 5000, 1).Should().Be(2000);
            algorithm.GetDisplayStartTime(17000, 0, 10000, 1).Should().Be(7000);
        }

        [Test]
        public void TestObjectDisplayStartTime()
        {
            algorithm.GetDisplayStartTime(2000, 50, 1000, 500).Should().Be(900); // 2000 - (1 + 50 / 500) * 1000
            algorithm.GetDisplayStartTime(10000, 50, 1000, 500).Should().Be(8900); // 10000 - (1 + 50 / 500) * 1000
            algorithm.GetDisplayStartTime(15000, 250, 1000, 500).Should().Be(13500); // 15000 - (1 + 250 / 500) * 1000
            algorithm.GetDisplayStartTime(25000, 100, 5000, 500).Should().Be(19000); // 25000 - (1 + 100 / 500) * 5000
        }

        [Test]
        public void TestLength()
        {
            algorithm.GetLength(0, 1000, 5000, 1).Should().Be(1f / 5);
            algorithm.GetLength(6000, 7000, 5000, 1).Should().Be(1f / 5);
        }

        [Test]
        public void TestPosition()
        {
            algorithm.PositionAt(1000, 0, 5000, 1).Should().Be(1f / 5);
            algorithm.PositionAt(6000, 5000, 5000, 1).Should().Be(1f / 5);
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(15000)]
        [TestCase(20000)]
        [TestCase(25000)]
        public void TestTime(double time)
        {
            algorithm.TimeAt(algorithm.PositionAt(time, 0, 5000, 1), 0, 5000, 1).Should().BeApproximately(time, 0.001);
            algorithm.TimeAt(algorithm.PositionAt(time, 5000, 5000, 1), 5000, 5000, 1).Should().BeApproximately(time, 0.001);
        }
    }
}
