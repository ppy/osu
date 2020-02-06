// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// Omits all decimal places when <paramref name="accuracy"/> equals 1d.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this double accuracy) => accuracy == 1 ? "100%" : $"{accuracy:0.00%}";

        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// Omits all decimal places when <paramref name="accuracy"/> equals 100m.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this decimal accuracy) => accuracy == 100 ? "100%" : $"{accuracy:0.00}%";
    }
}
