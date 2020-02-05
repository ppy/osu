// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <param name="alwaysShowDecimals">Whether to show decimal places if <paramref name="accuracy"/> equals 1d</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this double accuracy, bool alwaysShowDecimals = false) => accuracy == 1 && !alwaysShowDecimals ? "100%" : $"{accuracy:0.00%}";

        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted</param>
        /// <param name="alwaysShowDecimals">Whether to show decimal places if <paramref name="accuracy"/> equals 100m</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static string FormatAccuracy(this decimal accuracy, bool alwaysShowDecimals = false) => accuracy == 100 && !alwaysShowDecimals ? "100%" : $"{accuracy:0.00}%";
    }
}
