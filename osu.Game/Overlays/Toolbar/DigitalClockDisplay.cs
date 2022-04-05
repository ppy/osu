// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class DigitalClockDisplay : ClockDisplay
    {
        private bool format12Hour;

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

        public DigitalClockDisplay(bool format12Hour = false)
        {
            this.format12Hour = format12Hour;
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
            realTime.Text = format12Hour ? $"{now:hh:mm:ss tt}" : $"{now:HH:mm:ss}";
            gameTime.Text = $"running {new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
        }

        private void updateMetrics()
        {
            Width = showRuntime || format12Hour ? 66 : 45; // Allows for space for game time up to 99 days (in the padding area since this is quite rare).
            gameTime.FadeTo(showRuntime ? 1 : 0);
        }
    }
}
