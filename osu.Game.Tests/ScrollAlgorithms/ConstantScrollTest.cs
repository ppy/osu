// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public void TestDisplayStartTime()
        {
            Assert.AreEqual(-8000, algorithm.GetDisplayStartTime(2000, 10000));
            Assert.AreEqual(-3000, algorithm.GetDisplayStartTime(2000, 5000));
            Assert.AreEqual(2000, algorithm.GetDisplayStartTime(7000, 5000));
            Assert.AreEqual(7000, algorithm.GetDisplayStartTime(17000, 10000));
        }

        [Test]
        public void TestLength()
        {
            Assert.AreEqual(1f / 5, algorithm.GetLength(0, 1000, 5000, 1));
            Assert.AreEqual(1f / 5, algorithm.GetLength(6000, 7000, 5000, 1));
        }

        [Test]
        public void TestPosition()
        {
            Assert.AreEqual(1f / 5, algorithm.PositionAt(1000, 0, 5000, 1));
            Assert.AreEqual(1f / 5, algorithm.PositionAt(6000, 5000, 5000, 1));
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
