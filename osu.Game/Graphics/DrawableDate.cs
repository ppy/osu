// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public class DrawableDate : OsuSpriteText, IHasTooltip
    {
        private readonly DateTimeOffset date;
        private readonly String dateFormat;
        private readonly String tooltipFormat;

        public DrawableDate(DateTimeOffset date)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            // if date's format is not specified, set it to an empty string,
            // so that later we will know to humanize it
            this.dateFormat = "";
            // if tooltip's format is not specified, set it to an empty string
            // so later we will know to default to a default time format
            this.tooltipFormat = "";
        }

        public DrawableDate(string dateFormat, DateTimeOffset date)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            // set a date format for later from an argument
            this.dateFormat = dateFormat;
            // if tooltip's format is not specified, set it to an empty string,
            // so later we will know to default to a default time format
            this.tooltipFormat = "";
        }

        public DrawableDate(DateTimeOffset date, string tooltipFormat)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            // if date's format is not specified, set it to an empty string,
            // so that later we will know to humanize it
            this.dateFormat = "";
            // set a tooltip format for later from an argument
            this.tooltipFormat = tooltipFormat;
        }

        public DrawableDate(string dateFormat, DateTimeOffset date, string tooltipFormat)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            // set a date format for text generator from an argument
            this.dateFormat = dateFormat;
            // set a tooltip format for tooltip generator from an argument
            this.tooltipFormat = tooltipFormat;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateTime();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.Add(updateTimeWithReschedule);
        }

        private void updateTimeWithReschedule()
        {
            updateTime();

            var diffToNow = DateTimeOffset.Now.Subtract(date);

            double timeUntilNextUpdate = 1000;
            if (diffToNow.TotalSeconds > 60)
            {
                timeUntilNextUpdate *= 60;
                if (diffToNow.TotalMinutes > 60)
                {
                    timeUntilNextUpdate *= 60;

                    if (diffToNow.TotalHours > 24)
                        timeUntilNextUpdate *= 24;
                }
            }

            Scheduler.AddDelayed(updateTimeWithReschedule, timeUntilNextUpdate);
        }

        public override bool HandleMouseInput => true;

        // if date's format is specified
        private void updateTime() => Text = (dateFormat != "") ?
            // format it as requested in a passed argument
            (String.Format(dateFormat, date)) :
            // otherwise, humanize it (for example: 2 hours ago)
            (date.Humanize());

        // if we know that the tooltip format exists
        public string TooltipText => (tooltipFormat != "") ?
            // then we format the tooltip text using that format
            (String.Format(tooltipFormat, date)) :
            // but otherwise, simply convert the date to string
            (date.ToString());
    }
}
