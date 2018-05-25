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

        /// <param name="dateFormat">The string to format the date text with.
        /// May be null if the humanized format should be used.</param>
        public DrawableDate(DateTimeOffset date, string dateFormat = null)
        {
            AutoSizeAxes = Axes.Both;
            Font = "Exo2.0-RegularItalic";

            this.date = date.ToLocalTime();
            this.dateFormat = dateFormat;
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

        public string TooltipText => string.Format("{0:d MMMM yyyy H:mm \"UTC\"z}", date);
    }
}
