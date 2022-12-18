// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ProgressBar : SliderBar<double>
    {
        public Action<double> OnSeek;

        private readonly Box fill;
        private readonly Box background;

        public Color4 FillColour
        {
            set => fill.FadeColour(value, 150, Easing.OutQuint);
        }

        public Color4 BackgroundColour
        {
            set
            {
                background.Alpha = 1;
                background.Colour = value;
            }
        }

        public double EndTime
        {
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            set => CurrentNumber.Value = value;
        }

        private readonly bool allowSeek;

        public override bool HandlePositionalInput => allowSeek;
        public override bool HandleNonPositionalInput => allowSeek;

        /// <summary>
        /// Construct a new progress bar.
        /// </summary>
        /// <param name="allowSeek">Whether the user should be allowed to click/drag to adjust the value.</param>
        public ProgressBar(bool allowSeek)
        {
            this.allowSeek = allowSeek;

            CurrentNumber.MinValue = 0;
            CurrentNumber.MaxValue = 1;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                background = new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                },
                fill = new Box { RelativeSizeAxes = Axes.Y }
            };
        }

        protected override void UpdateValue(float value)
        {
            fill.Width = value * UsableWidth;
        }

        protected override void OnUserChange(double value) => OnSeek?.Invoke(value);
    }
}
