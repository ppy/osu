// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using NUnit.Framework;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class ParsingTest
    {
        [Test]
        public void TestNaNHandling() => allThrow<FormatException>("NaN");

        [Test]
        public void TestBadStringHandling() => allThrow<FormatException>("Random string 123");

        [TestCase(Parsing.MAX_PARSE_VALUE)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-Parsing.MAX_PARSE_VALUE)]
        [TestCase(10, 10)]
        [TestCase(-10, 10)]
        public void TestValidRanges(double input, double limit = Parsing.MAX_PARSE_VALUE)
        {
            Assert.AreEqual(Parsing.ParseInt((input).ToString(CultureInfo.InvariantCulture), (int)limit), (int)input);
            Assert.AreEqual(Parsing.ParseFloat((input).ToString(CultureInfo.InvariantCulture), (float)limit), (float)input);
            Assert.AreEqual(Parsing.ParseDouble((input).ToString(CultureInfo.InvariantCulture), limit), input);
        }

        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(999999999999)]
        [TestCase(Parsing.MAX_PARSE_VALUE * 1.1)]
        [TestCase(-Parsing.MAX_PARSE_VALUE * 1.1)]
        [TestCase(11, 10)]
        [TestCase(-11, 10)]
        public void TestOutOfRangeHandling(double input, double limit = Parsing.MAX_PARSE_VALUE)
            => allThrow<OverflowException>(input.ToString(CultureInfo.InvariantCulture), limit);

        private void allThrow<T>(string input, double limit = Parsing.MAX_PARSE_VALUE)
            where T : Exception
        {
            Assert.Throws(getIntParseException(input) ?? typeof(T), () => Parsing.ParseInt(input, (int)limit));
            Assert.Throws<T>(() => Parsing.ParseFloat(input, (float)limit));
            Assert.Throws<T>(() => Parsing.ParseDouble(input, limit));
        }

        /// <summary>
        /// <see cref="int"/> may not be able to parse some inputs.
        /// In this case we expect to receive the raw parsing exception.
        /// </summary>
        /// <param name="input">The input attempting to be parsed.</param>
        /// <returns>The type of exception thrown by <see cref="int.Parse(string)"/>. Null if no exception is thrown.</returns>
        private Type getIntParseException(string input)
        {
            try
            {
                var _ = int.Parse(input);
            }
            catch (Exception e)
            {
                return e.GetType();
            }

            return null;
        }
    }
}
