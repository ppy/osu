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
    }
}
