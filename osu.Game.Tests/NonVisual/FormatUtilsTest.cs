// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class FormatUtilsTest
    {
        [TestCase(0, "0.00%")]
        [TestCase(0.01, "1.00%")]
        [TestCase(0.9899, "98.99%")]
        [TestCase(0.989999, "98.99%")]
        [TestCase(0.99, "99.00%")]
        [TestCase(0.9999, "99.99%")]
        [TestCase(0.999999, "99.99%")]
        [TestCase(1, "100.00%")]
        public void TestAccuracyFormatting(double input, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, input.FormatAccuracy().ToString());
        }

        [TestCase(3, "3.00")]
        [TestCase(3.3, "3.30")]
        [TestCase(3.55, "3.55")]
        [TestCase(3.553, "3.55")]
        [TestCase(3.557, "3.55")]
        [TestCase(3.9999, "3.99")]
        [TestCase(3.999999, "3.99")]
        [TestCase(4, "4.00")]
        public void TestStarRatingFormatting(double input, string expectedOutput)
        {
            Assert.AreEqual(expectedOutput, input.FormatStarRating().ToString());
        }
    }
}
