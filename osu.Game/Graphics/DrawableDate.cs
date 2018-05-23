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
        private readonly string dateFormat;
        private readonly string tooltipFormat;

        /// <param name="dateFormat">The string to format the date text with.
        /// May be null if the humanized format should be used.</param>
        /// <param name="tooltipFormat">The string to format the tooltip text with.
        /// May be null if the default format should be used.</param>
        public DrawableDate(DateTimeOffset date, string dateFormat = null, string tooltipFormat = null)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            this.dateFormat = dateFormat;
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

        private void updateTime() => Text = string.IsNullOrEmpty(dateFormat) ?
            date.Humanize() :
            string.Format(dateFormat, date);

        public string TooltipText => string.IsNullOrEmpty(tooltipFormat) ?
            date.ToString() :
            string.Format(tooltipFormat, date);
    }
}
