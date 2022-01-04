// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;

namespace osu.Game.Extensions
{
    public static class TimeDisplayExtensions
    {
        /// <summary>
        /// Get an editor formatted string (mm:ss:mss)
        /// </summary>
        /// <param name="milliseconds">A time value in milliseconds.</param>
        /// <returns>An editor formatted display string.</returns>
        public static string ToEditorFormattedString(this double milliseconds) =>
            ToEditorFormattedString(TimeSpan.FromMilliseconds(milliseconds));

        /// <summary>
        /// Get an editor formatted string (mm:ss:mss)
        /// </summary>
        /// <param name="timeSpan">A time value.</param>
        /// <returns>An editor formatted display string.</returns>
        public static string ToEditorFormattedString(this TimeSpan timeSpan) =>
            $"{(timeSpan < TimeSpan.Zero ? "-" : string.Empty)}{(int)timeSpan.TotalMinutes:00}:{timeSpan:ss\\:fff}";

        /// <summary>
        /// Get a formatted duration (dd:hh:mm:ss with days/hours omitted if zero).
        /// </summary>
        /// <param name="milliseconds">A duration in milliseconds.</param>
        /// <returns>A formatted duration string.</returns>
        public static LocalisableString ToFormattedDuration(this double milliseconds) =>
            ToFormattedDuration(TimeSpan.FromMilliseconds(milliseconds));

        /// <summary>
        /// Get a formatted duration (dd:hh:mm:ss with days/hours omitted if zero).
        /// </summary>
        /// <param name="timeSpan">A duration value.</param>
        /// <returns>A formatted duration string.</returns>
        public static LocalisableString ToFormattedDuration(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return new LocalisableFormattableString(timeSpan, @"dd\:hh\:mm\:ss");

            if (timeSpan.TotalHours >= 1)
                return new LocalisableFormattableString(timeSpan, @"hh\:mm\:ss");

            return new LocalisableFormattableString(timeSpan, @"mm\:ss");
        }
    }
}
