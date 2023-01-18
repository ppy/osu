// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class TestSceneSnappingNearZero
    {
        private readonly ControlPointInfo cpi = new ControlPointInfo();

        [Test]
        public void TestOnZero()
        {
            test(0, 500, 0, 0);
            test(0, 500, 100, 0);
            test(0, 500, 250, 500);
            test(0, 500, 600, 500);

            test(0, 500, -600, 0);
        }

        [Test]
        public void TestAlmostOnZero()
        {
            test(50, 500, 0, 50);
            test(50, 500, 50, 50);
            test(50, 500, 100, 50);
            test(50, 500, 299, 50);
            test(50, 500, 300, 550);

            test(50, 500, -500, 50);
        }

        [Test]
        public void TestAlmostOnOne()
        {
            test(499, 500, -1, 499);
            test(499, 500, 0, 499);
            test(499, 500, 1, 499);
            test(499, 500, 499, 499);
            test(499, 500, 600, 499);
            test(499, 500, 800, 999);
        }

        [Test]
        public void TestOnOne()
        {
            test(500, 500, -500, 0);
            test(500, 500, 0, 0);
            test(500, 500, 200, 0);
            test(500, 500, 400, 500);
            test(500, 500, 500, 500);
            test(500, 500, 600, 500);
            test(500, 500, 900, 1000);
        }

        [Test]
        public void TestNegative()
        {
            test(-600, 500, -600, 400);
            test(-600, 500, -100, 400);
            test(-600, 500, 0, 400);
            test(-600, 500, 200, 400);
            test(-600, 500, 400, 400);
            test(-600, 500, 600, 400);
            test(-600, 500, 1000, 900);
        }

        private void test(double pointTime, double beatLength, double from, double expected)
        {
            cpi.Clear();
            cpi.Add(pointTime, new TimingControlPoint { BeatLength = beatLength });
            Assert.That(cpi.GetClosestSnappedTime(from, 1), Is.EqualTo(expected), $"From: {from}");
        }
    }
}
