// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public partial class DigitalClockDisplay : ClockDisplay
    {
        private OsuSpriteText realTime;
        private OsuSpriteText gameTime;

        private bool showRuntime = true;

        public bool ShowRuntime
        {
            get => showRuntime;
            set
            {
                if (showRuntime == value)
                    return;

                showRuntime = value;
                updateMetrics();
            }
        }

        private bool use24HourDisplay;

        public bool Use24HourDisplay
        {
            get => use24HourDisplay;
            set
            {
                if (use24HourDisplay == value)
                    return;

                use24HourDisplay = value;

                updateMetrics();
                UpdateDisplay(DateTimeOffset.Now); //Update realTime.Text immediately instead of waiting until next second
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                realTime = new OsuSpriteText(),
                gameTime = new OsuSpriteText
                {
                    Y = 14,
                    Colour = colours.PinkLight,
                    Font = OsuFont.Default.With(size: 10, weight: FontWeight.SemiBold),
                }
            };

            updateMetrics();
        }

        protected override void UpdateDisplay(DateTimeOffset now)
        {
            realTime.Text = now.ToLocalisableString(use24HourDisplay ? @"HH:mm:ss" : @"h:mm:ss tt");
            gameTime.Text = $"running {new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
        }

        private void updateMetrics()
        {
            Width = showRuntime || !use24HourDisplay ? 66 : 45; // Allows for space for game time up to 99 days (in the padding area since this is quite rare).

            gameTime.FadeTo(showRuntime ? 1 : 0);
        }
    }
}
