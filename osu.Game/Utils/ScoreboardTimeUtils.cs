// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Resources.Localisation.Web;

#nullable enable

namespace osu.Game.Utils
{
    public static class ScoreboardTimeUtils
    {
        /// <summary>
        /// Formats a provided date to a short relative string version for compact display.
        /// </summary>
        /// <param name="time">The time to be displayed.</param>
        /// <param name="lowerCutoff">A timespan denoting the time length beneath which "now" should be displayed.</param>
        /// <returns>A short relative string representing the input time.</returns>
        public static string FormatRelativeTime(DateTimeOffset time, TimeSpan lowerCutoff)
        {
            // web uses momentjs's custom locales to format the date for the purposes of the scoreboard.
            // this is intended to be a best-effort, more legible approximation of that.
            // compare:
            // * https://github.com/ppy/osu-web/blob/a8f5a68fb435cb19a4faa4c7c4bce08c4f096933/resources/assets/lib/scoreboard-time.tsx
            // * https://momentjs.com/docs/#/customization/ (reference for the customisation format)

            // TODO: support localisation (probably via `CommonStrings.CountHours()` etc.)
            // requires pluralisable string support framework-side

            var now = DateTime.Now;
            var span = now - time;

            if (span < lowerCutoff)
                return CommonStrings.TimeNow.ToString();

            if (span.TotalMinutes < 1)
                return formatQuantity("sec", (int)span.TotalSeconds);
            if (span.TotalHours < 1)
                return formatQuantity("min", (int)span.TotalMinutes);
            if (span.TotalDays < 1)
                return formatQuantity("hr", (int)span.TotalHours);

            // this is where this gets more complicated because of how the calendar works.
            // since there's no `TotalMonths` / `TotalYears`, we have to iteratively add months/years
            // and test against cutoff dates to determine how many months/years to show.

            if (time > now.AddMonths(-1))
                return formatQuantity("dy", (int)span.TotalDays);

            for (int months = 1; months <= 11; ++months)
            {
                if (time > now.AddMonths(-(months + 1)))
                    return formatQuantity("mo", months);
            }

            int years = 1;
            // Add upper bound to prevent a crash
            while (years < 20 && time <= now.AddYears(-(years + 1)))
                years += 1;
            if (years < 20)
                return formatQuantity("yr", years);

            return "never";
        }

        private static string formatQuantity(string template, int quantity)
        {
            if (quantity <= 1)
                return $@"{quantity}{template}";

            return $@"{quantity}{template}s";
        }
    }
}
