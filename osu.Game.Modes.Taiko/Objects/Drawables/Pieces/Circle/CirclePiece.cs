// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Ring;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle
{
    /// <summary>
    /// The HitObject circle piece, containing the outer glow, the inner circle, and the ring.
    /// </summary>
    public abstract class CirclePiece : Container
    {
        /// <summary>
        /// Whether the HitObject is in kiai time.
        /// </summary>
        public bool Kiai;

        /// <summary>
        /// The colour of the inner circle and glows.
        /// </summary>
        protected abstract Color4 InnerColour { get; set; }

        private CircularContainer backingGlowContainer;
        private CircularContainer circle;

        protected CirclePiece()
        {
            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                // The faint glow around the HitObject
                backingGlowContainer = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both
                },
                // The inner circle 
                circle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Children = new Drawable[]
                    {
                        // Background
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 1
                        },
                        // Triangles
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0.5f,
                            Colour = Color4.Black,

                            TriangleScale = 1.5f
                        },
                    }
                },
                // The ring
                CreateRing()
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            circle.Colour = InnerColour;

            backingGlowContainer.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = InnerColour,
                Radius = 8f
            };

            if (Kiai)
            {
                circle.EdgeEffect = new EdgeEffect
                {
                    Colour = InnerColour,
                    Radius = 50,
                    Type = EdgeEffectType.Glow,
                };
            }
        }

        /// <summary>
        /// Creates the outer + inner ring piece.
        /// </summary>
        /// <returns>The ring piece.</returns>
        protected abstract RingPiece CreateRing();
    }
}
