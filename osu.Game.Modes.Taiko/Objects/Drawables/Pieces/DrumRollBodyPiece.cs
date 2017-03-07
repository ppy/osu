// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// The internal coloured "bar" of a finisher drum roll.
    /// This overshoots the expected length by corner radius on both sides.
    /// </summary>
    public class DrumRollFinisherBodyPiece : DrumRollBodyPiece
    {
        public override float CornerRadius => base.CornerRadius * 1.5f;

        public DrumRollFinisherBodyPiece(float baseLength)
            : base(baseLength)
        {
            Size *= new Vector2(1, 1.5f);
        }
    }

    /// <summary>
    /// The internal coloured "bar" of a drum roll.
    /// This overshoots the expected length by corner radius on both sides.
    /// </summary>
    public class DrumRollBodyPiece : Container
    {
        /// <summary>
        /// Half the default (128) height.
        /// </summary>
        public override float CornerRadius => TaikoHitObject.CIRCLE_RADIUS;

        /// <summary>
        /// Whether the drum roll is in Kiai time.
        /// </summary>
        public bool Kiai;

        private Box background;

        public DrumRollBodyPiece(float baseLength)
        {
            Size = new Vector2(baseLength + TaikoHitObject.CIRCLE_RADIUS * 2, TaikoHitObject.CIRCLE_RADIUS * 2);

            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 4;

            Children = new Drawable[]
            {
                // Background
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // Triangles
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,

                    Colour = Color4.Black,
                    Alpha = 0.05f,

                    TriangleScale = 1.5f
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Yellow;

            if (Kiai)
            {
                EdgeEffect = new EdgeEffect
                {
                    Colour = new Color4(colours.Yellow.R, colours.Yellow.G, colours.Yellow.B, 0.75f),
                    Radius = 50,
                    Type = EdgeEffectType.Glow,
                };
            }
        }
    }
}
