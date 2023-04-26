// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using Humanizer;
using Humanizer.Localisation;

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
        /// Turns the current or provided timespan into a human readable sentence
        /// </summary>
        /// <param name="input">The date to be humanized</param>
        /// <param name="precision">The maximum number of time units to return. Defaulted is 1 which means the largest unit is returned</param>
        /// <param name="maxUnit">The maximum unit of time to output. The default value is <see cref="TimeUnit.Week"/>. The time units <see cref="TimeUnit.Month"/> and <see cref="TimeUnit.Year"/> will give approximations for time spans bigger 30 days by calculating with 365.2425 days a year and 30.4369 days a month.</param>
        /// <param name="minUnit">The minimum unit of time to output.</param>
        /// <param name="toWords">Uses words instead of numbers if true. E.g. one day.</param>
        /// <returns>distance of time in words</returns>
        public static string Humanize(TimeSpan input, int precision = 1, TimeUnit maxUnit = TimeUnit.Week, TimeUnit minUnit = TimeUnit.Millisecond, bool toWords = false)
        {
            // this works around https://github.com/xamarin/xamarin-android/issues/2012 and https://github.com/Humanizr/Humanizer/issues/690#issuecomment-368536282
            try
            {
                return input.Humanize(precision: precision, maxUnit: maxUnit, minUnit: minUnit);
            }
            catch (ArgumentException)
            {
                return input.Humanize(culture: new CultureInfo("en-US"), precision: precision, maxUnit: maxUnit, minUnit: minUnit);
            }
        }
    }
}
