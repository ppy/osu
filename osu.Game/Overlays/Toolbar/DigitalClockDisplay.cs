// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Toolbar
{
    public partial class DigitalClockDisplay : ClockDisplay
    {
        private OsuSpriteText realTime;
        private OsuSpriteText gameTime;

        private FillFlowContainer runningText;

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
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                realTime = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(fixedWidth: true),
                    Spacing = new Vector2(-1.5f, 0),
                },
                runningText = new FillFlowContainer
                {
                    Y = 14,
                    Colour = colours.PinkLight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "running",
                            Font = OsuFont.Default.With(size: 10, weight: FontWeight.SemiBold),
                        },
                        gameTime = new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(size: 10, fixedWidth: true, weight: FontWeight.SemiBold),
                            Spacing = new Vector2(-0.5f, 0),
                        }
                    }
                },
            };

            updateMetrics();
        }

        protected override void UpdateDisplay(DateTimeOffset now)
        {
            realTime.Text = now.ToLocalisableString(use24HourDisplay ? @"HH:mm:ss" : @"h:mm:ss tt");
            gameTime.Text = $"{new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
        }

        private void updateMetrics()
        {
            runningText.FadeTo(showRuntime ? 1 : 0);
        }
    }
}
