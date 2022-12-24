// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Audio;

namespace osu.Game.Tests.Audio
{
    [TestFixture]
    public class SampleInfoEqualityTest
    {
        [Test]
        public void TestSameSingleSamplesAreEqual()
        {
            var first = new SampleInfo("sample");
            var second = new SampleInfo("sample");

            assertEquality(first, second);
        }

        [Test]
        public void TestDifferentSingleSamplesAreNotEqual()
        {
            var first = new SampleInfo("first");
            var second = new SampleInfo("second");

            assertNonEquality(first, second);
        }

        [Test]
        public void TestDifferentCountSampleSetsAreNotEqual()
        {
            var first = new SampleInfo("sample", "extra");
            var second = new SampleInfo("sample");

            assertNonEquality(first, second);
        }

        [Test]
        public void TestDifferentSampleSetsOfSameCountAreNotEqual()
        {
            var first = new SampleInfo("first", "common");
            var second = new SampleInfo("common", "second");

            assertNonEquality(first, second);
        }

        [Test]
        public void TestSameOrderSameSampleSetsAreEqual()
        {
            var first = new SampleInfo("first", "second");
            var second = new SampleInfo("first", "second");

            assertEquality(first, second);
        }

        [Test]
        public void TestDifferentOrderSameSampleSetsAreEqual()
        {
            var first = new SampleInfo("first", "second");
            var second = new SampleInfo("second", "first");

            assertEquality(first, second);
        }

        private void assertEquality(SampleInfo first, SampleInfo second)
        {
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        private void assertNonEquality(SampleInfo first, SampleInfo second)
        {
            Assert.That(first.Equals(second), Is.False);
            Assert.That(first.GetHashCode(), Is.Not.EqualTo(second.GetHashCode()));
        }
    }
}
