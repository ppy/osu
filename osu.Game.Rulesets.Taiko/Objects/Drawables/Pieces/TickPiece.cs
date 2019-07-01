// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class TickPiece : TaikoPiece
    {
        /// <summary>
        /// Any tick that is not the first for a drumroll is not filled, but is instead displayed
        /// as a hollow circle. This is what controls the border width of that circle.
        /// </summary>
        private const float tick_border_width = 5;

        /// <summary>
        /// The size of a tick.
        /// </summary>
        private const float tick_size = 0.35f;

        private bool filled;

        public bool Filled
        {
            get => filled;
            set
            {
                filled = value;
                fillBox.Alpha = filled ? 1 : 0;
            }
        }

        private readonly Box fillBox;

        public TickPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            Size = new Vector2(tick_size);

            Add(new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = tick_border_width,
                BorderColour = Color4.White,
                Children = new[]
                {
                    fillBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            });
        }
    }
}
