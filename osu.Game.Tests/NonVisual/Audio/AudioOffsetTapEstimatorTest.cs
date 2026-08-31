// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Audio;

namespace osu.Game.Tests.NonVisual.Audio
{
    [TestFixture]
    public class AudioOffsetTapEstimatorTest
    {
        [TestCase(500, 45)]
        [TestCase(1000, -80)]
        public void TestEstimateAcrossLoop(int beatLength, double expectedError)
        {
            var estimator = new AudioOffsetTapEstimator();

            for (int i = 0; i < 24; i++)
            {
                double time = (250 + i * beatLength) % (16 * beatLength) + expectedError;
                Assert.That(estimator.AddTap(time, i * beatLength, beatLength, out _), Is.True);
            }

            Assert.That(estimator.Count, Is.EqualTo(16));
            Assert.That(estimator.MedianError, Is.EqualTo(expectedError).Within(0.001));
        }

        [Test]
        public void TestDoublePressesAndOutlier()
        {
            var estimator = new AudioOffsetTapEstimator();

            for (int i = 0; i < 8; i++)
            {
                double time = 250 + i * 1000 + (i == 3 ? 200 : 30);
                Assert.That(estimator.AddTap(time, i * 1000, 1000, out _), Is.True);
                Assert.That(estimator.AddTap(time, i * 1000 + 5, 1000, out _), Is.False);
                Assert.That(estimator.HasEstimate, Is.EqualTo(i == 7));
            }

            Assert.That(estimator.MedianError, Is.EqualTo(30).Within(0.001));
            estimator.Reset();
            Assert.That(estimator.Count, Is.Zero);
            Assert.That(estimator.HasEstimate, Is.False);
            Assert.That(estimator.AddTap(250, 0, 1000, out _), Is.True);
        }
    }
}
