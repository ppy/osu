// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Utils;

namespace osu.Game.Graphics
{
    public partial class DrawableDate : OsuSpriteText, IHasCustomTooltip<DateTimeOffset>
    {
        private DateTimeOffset date;

        public DateTimeOffset Date
        {
            get => date;
            set
            {
                if (date == value)
                    return;

                date = value.ToLocalTime();

                if (LoadState >= LoadState.Ready)
                    updateTime();
            }
        }

        public DrawableDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
        {
            Font = OsuFont.GetFont(weight: FontWeight.Regular, size: textSize, italics: italic);
            Date = date;
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

            if (Math.Abs(diffToNow.TotalSeconds) > 120)
            {
                timeUntilNextUpdate *= 60;

                if (Math.Abs(diffToNow.TotalMinutes) > 120)
                {
                    timeUntilNextUpdate *= 60;

                    if (Math.Abs(diffToNow.TotalHours) > 48)
                        timeUntilNextUpdate *= 24;
                }
            }

            Scheduler.AddDelayed(updateTimeWithReschedule, timeUntilNextUpdate);
        }

        protected virtual LocalisableString Format() => new LocalisableString(new HumanisedDate(Date));

        private void updateTime() => Text = Format();

        public ITooltip<DateTimeOffset> GetCustomTooltip() => new DateTooltip();

        public DateTimeOffset TooltipContent => Date;

        private class HumanisedDate : ILocalisableStringData
        {
            public readonly DateTimeOffset Date;

            public HumanisedDate(DateTimeOffset date)
            {
                Date = date;
            }

            /// <remarks>
            /// Humanizer formats the <see cref="Date"/> relative to the local computer time.
            /// Therefore, replacing a <see cref="HumanisedDate"/> instance with another instance of the class with the same <see cref="Date"/>
            /// should have the effect of replacing and re-formatting the text.
            /// Including <see cref="Date"/> in equality members would stop this from happening, as <see cref="SpriteText.Text"/>
            /// has equality-based early guards to prevent redundant text replaces.
            /// Thus, instances of these class just compare <see langword="false"/> to any <see cref="ILocalisableStringData"/> to ensure re-formatting happens correctly.
            /// There are "technically" more "correct" ways to do this (like also including the current time into equality checks),
            /// but they are simultaneously functionally equivalent to this and overly convoluted.
            /// This is a private hack-job of a wrapper around humanizer anyway.
            /// </remarks>
            public bool Equals(ILocalisableStringData? other) => false;

            public string GetLocalised(LocalisationParameters parameters) => HumanizerUtils.Humanize(Date);

            public override string ToString() => GetLocalised(LocalisationParameters.DEFAULT);
        }
    }
}
