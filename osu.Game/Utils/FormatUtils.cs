// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this double accuracy) => $"{accuracy:0.00%}";

        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this decimal accuracy) => $"{accuracy:0.00}%";

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 100_000 ? 1 : 0);
    }
}
