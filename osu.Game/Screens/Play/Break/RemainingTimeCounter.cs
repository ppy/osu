// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.Break
{
    public partial class RemainingTimeCounter : CompositeDrawable
    {
        private readonly OsuSpriteText counter;

        private double remainingTime;
        private int displayedSeconds = -1;

        /// <summary>
        /// The amount of time left to display, in milliseconds.
        /// </summary>
        public double RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;

                int seconds = (int)Math.Ceiling(value / 1000);

                if (seconds == displayedSeconds)
                    return;

                displayedSeconds = seconds;
                counter.Text = seconds.ToString();
            }
        }

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = counter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 33),
            };
        }
    }
}
