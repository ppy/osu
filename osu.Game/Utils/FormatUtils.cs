// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted.</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static LocalisableString FormatAccuracy(this double accuracy)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" percentage to the next whole number.
            // ie. a score which gets 89.99999% shouldn't ever show as 90%.
            // the reasoning for this is that cutoffs for grade increases are at whole numbers and displaying the required
            // percentile with a non-matching grade is confusing.
            accuracy = Math.Floor(accuracy * 10000) / 10000;

            return accuracy.ToLocalisableString("0.00%");
        }

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 100_000 ? 1 : 0);

        /// <summary>
        /// Finds the number of digits after the decimal.
        /// </summary>
        /// <param name="d">The value to find the number of decimal digits for.</param>
        /// <returns>The number decimal digits.</returns>
        public static int FindPrecision(decimal d)
        {
            int precision = 0;

            while (d != Math.Round(d))
            {
                d *= 10;
                precision++;
            }

            return precision;
        }
    }
}
