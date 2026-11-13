// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

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

            // formatting mirrors the osu-web scoreboard, reusing its (already localised) `common.scoreboard_time.*` strings.
            // these are in moment.js format: a `%d` count placeholder plus a separate singular/plural variant per unit.
            // compare: https://github.com/ppy/osu-web/blob/a8f5a68fb435cb19a4faa4c7c4bce08c4f096933/resources/assets/lib/scoreboard-time.tsx

            if (difference < lowerCutoff)
                return CommonStrings.TimeNow;

            if (difference.TotalMinutes < 1)
                return scoreboardTime((int)difference.TotalSeconds, CommonStrings.ScoreboardTimes, CommonStrings.ScoreboardTimes);
            if (difference.TotalHours < 1)
                return scoreboardTime((int)difference.TotalMinutes, CommonStrings.ScoreboardTimem, CommonStrings.ScoreboardTimemm);
            if (difference.TotalDays < 1)
                return scoreboardTime((int)difference.TotalHours, CommonStrings.ScoreboardTimeh, CommonStrings.ScoreboardTimehh);

            // this is where this gets more complicated because of how the calendar works.
            // since there's no `TotalMonths` / `TotalYears`, we have to iteratively add months/years
            // and test against cutoff dates to determine how many months/years to show.

            if (time > now.AddMonths(-1))
                return scoreboardTime((int)difference.TotalDays, CommonStrings.ScoreboardTimed, CommonStrings.ScoreboardTimedd);

            for (int months = 1; months <= 11; ++months)
            {
                if (time > now.AddMonths(-(months + 1)))
                    return scoreboardTime(months, CommonStrings.ScoreboardTimeMonth, CommonStrings.ScoreboardTimeMonths);
            }

            int years = 1;
            while (time <= now.AddYears(-(years + 1)))
                years += 1;
            return scoreboardTime(years, CommonStrings.ScoreboardTimey, CommonStrings.ScoreboardTimeyy);
        }

        // moment.js uses the singular variant when the (rounded) count is exactly 1, and the plural variant otherwise.
        private static LocalisableString scoreboardTime(int count, LocalisableString singular, LocalisableString plural)
            => new LocalisableString(new ScoreboardTimeString(count == 1 ? singular : plural, count));

        /// <summary>
        /// Resolves an osu-web scoreboard time unit string (moment.js format, e.g. <c>"%dd"</c> or <c>"now"</c>),
        /// substituting the moment.js <c>%d</c> count placeholder for <see cref="Count"/> in the target culture.
        /// </summary>
        private class ScoreboardTimeString : ILocalisableStringData
        {
            public readonly LocalisableString Unit;
            public readonly int Count;

            public ScoreboardTimeString(LocalisableString unit, int count)
            {
                Unit = unit;
                Count = count;
            }

            public string GetLocalised(LocalisationParameters parameters)
            {
                // `Unit`'s underlying data is not accessible from here, so it is resolved via a non-splitting `PluralisableString`.
                string format = new PluralisableString(Unit, Count, '\0').GetLocalised(parameters);
                return format.Replace(@"%d", Count.ToString(parameters.Store?.EffectiveCulture ?? CultureInfo.CurrentCulture));
            }

            public override string ToString() => GetLocalised(LocalisationParameters.DEFAULT);

            public bool Equals(ILocalisableStringData? other) => other is ScoreboardTimeString s && Count == s.Count && Unit.Equals(s.Unit);
        }
    }
}
