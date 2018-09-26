// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics
{
    public class DrawableDate : OsuSpriteText, IHasTooltip
    {
        protected readonly DateTimeOffset Date;

        public DrawableDate(DateTimeOffset date)
        {
            Font = "Exo2.0-RegularItalic";

            Date = date.ToLocalTime();
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

            var diffToNow = DateTimeOffset.Now.Subtract(Date);

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

        public override bool HandlePositionalInput => true;

        protected virtual string Format() => Date.Humanize();

        private void updateTime() => Text = Format();

        public virtual string TooltipText => string.Format($"{Date:MMMM d, yyyy h:mm tt \"UTC\"z}");
    }
}
