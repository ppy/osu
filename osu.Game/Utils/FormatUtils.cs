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
        public static double FloorToDecimalDigits(this double value, uint digits)
        {
            double base10 = Math.Pow(10, digits);
            return Math.Floor(value * base10) / base10;
        }

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
            return accuracy.FloorToDecimalDigits(4).ToLocalisableString("0.00%");
        }

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 10_000 ? 1 : 0);

        /// <summary>
        /// Formats the supplied star rating in a consistent, simplified way.
        /// </summary>
        /// <param name="starRating">The star rating to be formatted.</param>
        public static LocalisableString FormatStarRating(this double starRating)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" star rating to the next whole number.
            // i.e. a beatmap which has a star rating of 6.9999* should never show as 7.00*.
            // this matters for star rating medals which use hard cutoffs at whole numbers,
            // which then confuses users when they beat a 6.9999* beatmap but don't get the 7-star medal.
            return starRating.FloorToDecimalDigits(2).ToLocalisableString("0.00");
        }

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

        /// <summary>
        /// Applies rounding to the given BPM value.
        /// </summary>
        /// <param name="baseBpm">The base BPM to round.</param>
        /// <param name="rate">Rate adjustment, if applicable.</param>
        public static int RoundBPM(double baseBpm, double rate = 1) => (int)Math.Round(baseBpm * rate);
    }
}
