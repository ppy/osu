// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreboardTime : DrawableDate
    {
        public ScoreboardTime(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
            : base(date, textSize, italic)
        {
        }

        protected override string Format()
        {
            var now = DateTime.Now;
            var difference = now - Date;

            // web uses momentjs's custom locales to format the date for the purposes of the scoreboard.
            // this is intended to be a best-effort, more legible approximation of that.
            // compare:
            // * https://github.com/ppy/osu-web/blob/a8f5a68fb435cb19a4faa4c7c4bce08c4f096933/resources/assets/lib/scoreboard-time.tsx
            // * https://momentjs.com/docs/#/customization/ (reference for the customisation format)

            // TODO: support localisation (probably via `CommonStrings.CountHours()` etc.)
            // requires pluralisable string support framework-side

            if (difference.TotalHours < 1)
                return CommonStrings.TimeNow.ToString();
            if (difference.TotalDays < 1)
                return "hr".ToQuantity((int)difference.TotalHours);

            // this is where this gets more complicated because of how the calendar works.
            // since there's no `TotalMonths` / `TotalYears`, we have to iteratively add months/years
            // and test against cutoff dates to determine how many months/years to show.

            if (Date > now.AddMonths(-1))
                return difference.TotalDays < 2 ? "1dy" : $"{(int)difference.TotalDays}dys";

            for (int months = 1; months <= 11; ++months)
            {
                if (Date > now.AddMonths(-(months + 1)))
                    return months == 1 ? "1mo" : $"{months}mos";
            }

            int years = 1;
            while (Date <= now.AddYears(-(years + 1)))
                years += 1;
            return years == 1 ? "1yr" : $"{years}yrs";
        }
    }
}
