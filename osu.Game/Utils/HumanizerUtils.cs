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
        /// Humanizes a string using the system culture, then falls back if one cannot be found.
        /// <remarks>
        /// A localization lookup failure will throw an exception of type <see cref="ArgumentException"/>
        /// </remarks>
        /// </summary>
        /// <param name="dateTimeOffset">The time to humanize.</param>
        /// <returns>A humanized string of the given time.</returns>
        public static string Humanize(DateTimeOffset dateTimeOffset)
        {
            string offset;

            try
            {
                offset = dateTimeOffset.Humanize();
            }
            catch (ArgumentException)
            {
                offset = dateTimeOffset.Humanize(culture: new CultureInfo("en-US"));
            }

            return offset;
        }
    }
}
