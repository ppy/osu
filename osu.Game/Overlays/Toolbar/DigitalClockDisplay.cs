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
            realTime.Text = $"{now:HH:mm:ss}";
            gameTime.Text = $"running {new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
        }

        private void updateMetrics()
        {
            Width = showRuntime ? 66 : 45; // Allows for space for game time up to 99 days (in the padding area since this is quite rare).
            gameTime.FadeTo(showRuntime ? 1 : 0);
        }
    }
}
