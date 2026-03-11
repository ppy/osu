// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Extensions;

namespace osu.Game.Tests.Extensions
{
    [TestFixture]
    public class NumberFormattingExtensionsTest
    {
        [TestCase(-1, false, 0, ExpectedResult = "-1")]
        [TestCase(0, false, 0, ExpectedResult = "0")]
        [TestCase(1, false, 0, ExpectedResult = "1")]
        [TestCase(500, false, 10, ExpectedResult = "500")]
        [TestCase(-1, true, 0, ExpectedResult = "-1%")]
        [TestCase(0, true, 0, ExpectedResult = "0%")]
        [TestCase(1, true, 0, ExpectedResult = "1%")]
        [TestCase(50, true, 0, ExpectedResult = "50%")]
        [SetCulture("")] // invariant culture
        public string TestInteger(int input, bool percent, int decimalDigits)
        {
            return input.ToStandardFormattedString(decimalDigits, percent);
        }

        [TestCase(-1, false, 0, ExpectedResult = "-1")]
        [TestCase(-1e-6, false, 0, ExpectedResult = "0")]
        [TestCase(-1e-6, false, 6, ExpectedResult = "-0.000001")]
        [TestCase(0, false, 10, ExpectedResult = "0")]
        [TestCase(0, false, 0, ExpectedResult = "0")]
        [TestCase(double.NegativeZero, false, 0, ExpectedResult = "0")]
        [TestCase(1e-6, false, 0, ExpectedResult = "0")]
        [TestCase(1e-6, false, 6, ExpectedResult = "0.000001")]
        [TestCase(1, false, 0, ExpectedResult = "1")]
        [TestCase(1.528, false, 2, ExpectedResult = "1.53")]
        [TestCase(500, false, 10, ExpectedResult = "500")]
        [TestCase(-0.1, true, 0, ExpectedResult = "-10%")]
        [TestCase(0, true, 0, ExpectedResult = "0%")]
        [TestCase(0.4, true, 0, ExpectedResult = "40%")]
        [TestCase(0.48333, true, 2, ExpectedResult = "48%")]
        [TestCase(0.48333, true, 4, ExpectedResult = "48.33%")]
        [TestCase(1, true, 0, ExpectedResult = "100%")]
        [SetCulture("")] // invariant culture
        public string TestDouble(double input, bool percent, int decimalDigits)
        {
            return input.ToStandardFormattedString(decimalDigits, percent);
        }

        [Test]
        [SetCulture("fr-FR")]
        [TestCase(0.4, true, 2, ExpectedResult = "40%")]
        [TestCase(1e-6, false, 6, ExpectedResult = "0,000001")]
        [TestCase(0.48333, true, 4, ExpectedResult = "48,33%")]
        public string TestCultureSensitivity(double input, bool percent, int decimalDigits)
        {
            return input.ToStandardFormattedString(decimalDigits, percent);
        }
    }
}
