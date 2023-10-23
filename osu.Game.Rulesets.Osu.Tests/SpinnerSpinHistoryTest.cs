// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class SpinnerSpinHistoryTest
    {
        private SpinnerSpinHistory history = null!;

        [SetUp]
        public void Setup()
        {
            history = new SpinnerSpinHistory();
        }

        [TestCase(0, 0)]
        [TestCase(10, 10)]
        [TestCase(180, 180)]
        [TestCase(350, 350)]
        [TestCase(360, 360)]
        [TestCase(370, 370)]
        [TestCase(540, 540)]
        [TestCase(720, 720)]
        // ---
        [TestCase(-0, 0)]
        [TestCase(-10, 10)]
        [TestCase(-180, 180)]
        [TestCase(-350, 350)]
        [TestCase(-360, 360)]
        [TestCase(-370, 370)]
        [TestCase(-540, 540)]
        [TestCase(-720, 720)]
        public void TestSpinOneDirection(float spin, float expectedRotation)
        {
            history.ReportDelta(500, spin);
            Assert.That(history.TotalRotation, Is.EqualTo(expectedRotation));
        }

        [TestCase(0, 0, 0, 0)]
        // ---
        [TestCase(10, -10, 0, 10)]
        [TestCase(-10, 10, 0, 10)]
        // ---
        [TestCase(10, -20, 0, 10)]
        [TestCase(-10, 20, 0, 10)]
        // ---
        [TestCase(20, -10, 0, 20)]
        [TestCase(-20, 10, 0, 20)]
        // ---
        [TestCase(10, -360, 0, 350)]
        [TestCase(-10, 360, 0, 350)]
        // ---
        [TestCase(360, -10, 0, 370)]
        [TestCase(360, 10, 0, 370)]
        [TestCase(-360, 10, 0, 370)]
        [TestCase(-360, -10, 0, 370)]
        // ---
        [TestCase(10, 10, 10, 30)]
        [TestCase(10, 10, -10, 20)]
        [TestCase(10, -10, 10, 10)]
        [TestCase(-10, -10, -10, 30)]
        [TestCase(-10, -10, 10, 20)]
        [TestCase(-10, 10, 10, 10)]
        // ---
        [TestCase(10, -20, -350, 360)]
        [TestCase(10, -20, 350, 340)]
        [TestCase(-10, 20, 350, 360)]
        [TestCase(-10, 20, -350, 340)]
        public void TestSpinMultipleDirections(float spin1, float spin2, float spin3, float expectedRotation)
        {
            history.ReportDelta(500, spin1);
            history.ReportDelta(1000, spin2);
            history.ReportDelta(1500, spin3);
            Assert.That(history.TotalRotation, Is.EqualTo(expectedRotation));
        }

        // One spin
        [TestCase(370, -50, 320)]
        [TestCase(-370, 50, 320)]
        // Two spins
        [TestCase(740, -420, 320)]
        [TestCase(-740, 420, 320)]
        public void TestRemoveAndCrossFullSpin(float deltaToAdd, float deltaToRemove, float expectedRotation)
        {
            history.ReportDelta(1000, deltaToAdd);
            history.ReportDelta(500, deltaToRemove);
            Assert.That(history.TotalRotation, Is.EqualTo(expectedRotation));
        }

        // One spin + partial
        [TestCase(400, -30, -50, 320)]
        [TestCase(-400, 30, 50, 320)]
        // Two spins + partial
        [TestCase(800, -430, -50, 320)]
        [TestCase(-800, 430, 50, 320)]
        public void TestRemoveAndCrossFullAndPartialSpins(float deltaToAdd1, float deltaToAdd2, float deltaToRemove, float expectedRotation)
        {
            history.ReportDelta(1000, deltaToAdd1);
            history.ReportDelta(1500, deltaToAdd2);
            history.ReportDelta(500, deltaToRemove);
            Assert.That(history.TotalRotation, Is.EqualTo(expectedRotation));
        }

        [Test]
        public void TestRewindMultipleFullSpins()
        {
            history.ReportDelta(500, 360);
            history.ReportDelta(1000, 720);

            Assert.That(history.TotalRotation, Is.EqualTo(1080));

            history.ReportDelta(250, -900);

            Assert.That(history.TotalRotation, Is.EqualTo(180));
        }

        [Test]
        public void TestRewindOverDirectionChange()
        {
            history.ReportDelta(1000, 40); // max is now CW 40 degrees
            Assert.That(history.TotalRotation, Is.EqualTo(40));
            history.ReportDelta(1100, -90); // max is now CCW 50 degrees
            Assert.That(history.TotalRotation, Is.EqualTo(50));
            history.ReportDelta(1200, 110); // max is now CW 60 degrees
            Assert.That(history.TotalRotation, Is.EqualTo(60));

            history.ReportDelta(1000, -20);
            Assert.That(history.TotalRotation, Is.EqualTo(40));
        }
    }
}
