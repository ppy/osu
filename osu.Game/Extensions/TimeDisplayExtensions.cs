// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

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
                return timeSpan.ToLocalisableString(@"dd\:hh\:mm\:ss");

            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToLocalisableString(@"hh\:mm\:ss");

            return timeSpan.ToLocalisableString(@"mm\:ss");
        }

        /// <summary>
        /// Formats a provided date to a short relative string version for compact display.
        /// </summary>
        /// <param name="time">The time to be displayed.</param>
        /// <param name="lowerCutoff">A timespan denoting the time length beneath which "now" should be displayed.</param>
        /// <returns>A short relative string representing the input time.</returns>
        public static LocalisableString ToShortRelativeTime(this DateTimeOffset time, TimeSpan lowerCutoff)
        {
            // covers all `DateTimeOffset` instances with the date portion of 0001-01-01.
            if (time.Date == default)
                return "-";

            var now = DateTime.Now;
            var difference = now - time;

            // web uses momentjs's custom locales to format the date for the purposes of the scoreboard.
            // this is intended to be a best-effort, more legible approximation of that.
            // compare:
            // * https://github.com/ppy/osu-web/blob/a8f5a68fb435cb19a4faa4c7c4bce08c4f096933/resources/assets/lib/scoreboard-time.tsx
            // * https://momentjs.com/docs/#/customization/ (reference for the customisation format)

            if (difference < lowerCutoff)
                return WebCommonStrings.TimeNow;

            if (difference.TotalMinutes < 1)
            {
                int localDifference = (int)difference.TotalSeconds;
                return TimeDisplayStrings.TimeDisplay(localDifference, WebCommonStrings.CountSecondShortUnit.ToQuantity(localDifference));
            }

            if (difference.TotalHours < 1)
            {
                int localDifference = (int)difference.TotalMinutes;
                return TimeDisplayStrings.TimeDisplay(localDifference, WebCommonStrings.CountMinuteShortUnit.ToQuantity(localDifference));
            }

            if (difference.TotalDays < 1)
            {
                int localDifference = (int)difference.TotalHours;
                return TimeDisplayStrings.TimeDisplay(localDifference, WebCommonStrings.CountHourShortUnit.ToQuantity(localDifference));
            }

            // this is where this gets more complicated because of how the calendar works.
            // since there's no `TotalMonths` / `TotalYears`, we have to iteratively add months/years
            // and test against cutoff dates to determine how many months/years to show.

            if (time > now.AddMonths(-1))
            {
                int localDifference = (int)difference.TotalDays;
                return TimeDisplayStrings.TimeDisplay(localDifference, TimeDisplayStrings.CountDayShortUnit(localDifference));
            }

            for (int months = 1; months <= 11; ++months)
            {
                if (time > now.AddMonths(-(months + 1)))
                    return TimeDisplayStrings.TimeDisplay(months, TimeDisplayStrings.CountMonthShortUnit(months));
            }

            int years = 1;
            while (time <= now.AddYears(-(years + 1)))
                years += 1;
            return TimeDisplayStrings.TimeDisplay(years, TimeDisplayStrings.CountYearShortUnit(years));
        }
    }
}
