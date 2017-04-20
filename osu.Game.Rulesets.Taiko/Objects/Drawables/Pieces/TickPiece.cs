// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class TickPiece : TaikoPiece
    {
        /// <summary>
        /// Any tick that is not the first for a drumroll is not filled, but is instead displayed
        /// as a hollow circle. This is what controls the border width of that circle.
        /// </summary>
        private const float tick_border_width = TaikoHitObject.DEFAULT_CIRCLE_DIAMETER / 16;

        /// <summary>
        /// The size of a tick.
        /// </summary>
        private const float tick_size = TaikoHitObject.DEFAULT_CIRCLE_DIAMETER / 6;

        private bool filled;
        public bool Filled
        {
            get { return filled; }
            set
            {
                filled = value;
                fillBox.Alpha = filled ? 1 : 0;
            }
        }

        private readonly Box fillBox;

        public TickPiece()
        {
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
