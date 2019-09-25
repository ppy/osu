// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using Humanizer;

namespace osu.Game.Utils
{
    public static class HumanizerUtils
    {
        /// <summary>
        /// Turns the current or provided date into a human readable sentence
        /// </summary>
        /// <param name="input">The date to be humanized</param>
        /// <returns>distance of time in words</returns>
        public static string Humanize(DateTimeOffset input)
        {
            // this works around https://github.com/xamarin/xamarin-android/issues/2012 and https://github.com/Humanizr/Humanizer/issues/690#issuecomment-368536282
            try
            {
                return input.Humanize();
            }
            catch (ArgumentException)
            {
                return input.Humanize(culture: new CultureInfo("en-US"));
            }
        }

        /// <summary>
        /// Turns the current or provided big number into a readable string
        /// </summary>
        /// <param name="input">The number to be humanized</param>
        /// <returns>simplified number with a suffix</returns>
        public static string Humanize(long input)
        {
            const int k = 1000;

            var suffixes = new[]
            {
                "",
                "k",
                "million",
                "billion",
                "trillion",
                "quadrillion",
                "quintillion",
            };

            if (input < k)
                return input.ToString();

            int i = (int)Math.Floor(Math.Log(input, k));
            return $"{input / Math.Pow(k, i):F} {suffixes[i]}";
        }
    }
}
