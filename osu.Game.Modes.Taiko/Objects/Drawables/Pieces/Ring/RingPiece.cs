// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Ring
{
    /// <summary>
    /// The circle ring, containing both the outer and inner "rings".
    /// <para>The inner ring overridable and is piece-dependent.</para>
    /// </summary>
    public abstract class RingPiece : CircularContainer
    {
        public RingPiece()
        {
            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            BorderThickness = 8f;
            BorderColour = Color4.White;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0,
                    AlwaysPresent = true
                },
                CreateInnerPiece()
            };
        }

        /// <summary>
        /// Creates the inner "ring".
        /// </summary>
        /// <returns>The inner ring.</returns>
        protected abstract Drawable CreateInnerPiece();
    }
}
