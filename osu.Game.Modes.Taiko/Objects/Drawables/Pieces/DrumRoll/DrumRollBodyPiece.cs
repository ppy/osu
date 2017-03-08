// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.DrumRoll
{
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

        private CircularContainer backingGlowContainer;
        private CircularContainer backgroundContainer;
        private Box background;

        public DrumRollBodyPiece(float baseLength)
        {
            Size = new Vector2(baseLength + TaikoHitObject.CIRCLE_RADIUS * 2, TaikoHitObject.CIRCLE_RADIUS * 2);

            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;

            Children = new[]
            {
                backingGlowContainer = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                backgroundContainer = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    BorderColour = Color4.White,
                    BorderThickness = 8,

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
                            Alpha = 0.5f,

                            TriangleScale = 1.5f
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.YellowDark;

            backingGlowContainer.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = colours.YellowDark,
                Radius = 8f
            };

            if (Kiai)
            {
                backgroundContainer.EdgeEffect = new EdgeEffect
                {
                    Colour = colours.YellowDark,
                    Radius = 50,
                    Type = EdgeEffectType.Glow,
                };
            }
        }
    }
}
