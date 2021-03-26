// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using Humanizer;

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted.</param>
        /// <param name="formatProvider">An optional format provider.</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this double accuracy, IFormatProvider formatProvider = null)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" percentage to the next whole number.
            // ie. a score which gets 89.99999% shouldn't ever show as 90%.
            // the reasoning for this is that cutoffs for grade increases are at whole numbers and displaying the required
            // percentile with a non-matching grade is confusing.
            accuracy = Math.Floor(accuracy * 10000) / 10000;

            return accuracy.ToString("0.00%", formatProvider ?? CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 100_000 ? 1 : 0);
    }
}
