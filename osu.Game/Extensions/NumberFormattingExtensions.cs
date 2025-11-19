// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Numerics;
using osu.Game.Utils;

namespace osu.Game.Extensions
{
    public static class NumberFormattingExtensions
    {
        /// <summary>
        /// For a given numeric type, return a formatted string in the standard format we use for display everywhere.
        /// </summary>
        /// <remarks>
        /// Number formatting will abide by <see cref="CultureInfo.CurrentCulture"/>.
        /// </remarks>
        /// <param name="value">The numeric value.</param>
        /// <param name="maxDecimalDigits">The maximum number of decimals to be considered in the original value.</param>
        /// <param name="asPercentage">Whether the output should be a percentage. For integer types, 0-100 is mapped to 0-100%; for other types 0-1 is mapped to 0-100%.</param>
        /// <returns>The formatted output.</returns>
        public static string ToStandardFormattedString<T>(this T value, int maxDecimalDigits, bool asPercentage = false) where T : struct, INumber<T>, IMinMaxValue<T>
        {
            double floatValue = double.CreateTruncating(value);

            decimal decimalPrecision = normalise(decimal.CreateTruncating(value), maxDecimalDigits);

            // Find the number of significant digits (we could have less than maxDecimalDigits after normalize())
            int significantDigits = FormatUtils.FindPrecision(decimalPrecision);

            if (asPercentage)
            {
                if (value is int)
                    floatValue /= 100;

                return floatValue.ToString($@"0.{new string('0', Math.Max(0, significantDigits - 2))}%", CultureInfo.CurrentCulture);
            }

            string negativeSign = Math.Round(floatValue, significantDigits) < 0 ? "-" : string.Empty;

            return $"{negativeSign}{Math.Abs(floatValue).ToString($"N{significantDigits}", CultureInfo.CurrentCulture)}";
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private static decimal normalise(decimal d, int sd)
            => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
