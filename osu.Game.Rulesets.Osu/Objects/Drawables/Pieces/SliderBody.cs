// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public abstract class SliderBody : CompositeDrawable
    {
        public const float DEFAULT_BORDER_SIZE = 1;

        private SliderPath path;

        protected Path Path => path;

        public virtual float PathRadius
        {
            get => path.PathRadius;
            set => path.PathRadius = value;
        }

        /// <summary>
        /// Offset in absolute coordinates from the start of the curve.
        /// </summary>
        public virtual Vector2 PathOffset => path.PositionInBoundingBox(path.Vertices[0]);

        /// <summary>
        /// Used to colour the path.
        /// </summary>
        public Color4 AccentColour
        {
            get => path.AccentColour;
            set
            {
                if (path.AccentColour == value)
                    return;

                path.AccentColour = value;
            }
        }

        /// <summary>
        /// Used to colour the path border.
        /// </summary>
        public new Color4 BorderColour
        {
            get => path.BorderColour;
            set
            {
                if (path.BorderColour == value)
                    return;

                path.BorderColour = value;
            }
        }

        /// <summary>
        /// Used to size the path border.
        /// </summary>
        public float BorderSize
        {
            get => path.BorderSize;
            set
            {
                if (path.BorderSize == value)
                    return;

                path.BorderSize = value;
            }
        }

        protected SliderBody()
        {
            RecyclePath();
        }

        /// <summary>
        /// Initialises a new <see cref="SliderPath"/>, releasing all resources retained by the old one.
        /// </summary>
        public virtual void RecyclePath()
        {
            InternalChild = path = new SliderPath
            {
                Position = path?.Position ?? Vector2.Zero,
                PathRadius = path?.PathRadius ?? 10,
                AccentColour = path?.AccentColour ?? Color4.White,
                BorderColour = path?.BorderColour ?? Color4.White,
                BorderSize = path?.BorderSize ?? DEFAULT_BORDER_SIZE,
                Vertices = path?.Vertices ?? Array.Empty<Vector2>()
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => path.ReceivePositionalInputAt(screenSpacePos);

        /// <summary>
        /// Sets the vertices of the path which should be drawn by this <see cref="SliderBody"/>.
        /// </summary>
        /// <param name="vertices">The vertices</param>
        protected void SetVertices(IReadOnlyList<Vector2> vertices) => path.Vertices = vertices;

        private class SliderPath : SmoothPath
        {
            private const float border_max_size = 8f;
            private const float border_min_size = 0f;

            private const float border_portion = 0.128f;
            private const float gradient_portion = 1 - border_portion;

            private const float opacity_at_centre = 0.3f;
            private const float opacity_at_edge = 0.8f;

            private Color4 borderColour = Color4.White;

            public Color4 BorderColour
            {
                get => borderColour;
                set
                {
                    if (borderColour == value)
                        return;

                    borderColour = value;

                    InvalidateTexture();
                }
            }

            private Color4 accentColour = Color4.White;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    if (accentColour == value)
                        return;

                    accentColour = value;

                    InvalidateTexture();
                }
            }

            private float borderSize = DEFAULT_BORDER_SIZE;

            public float BorderSize
            {
                get => borderSize;
                set
                {
                    if (borderSize == value)
                        return;

                    if (value < border_min_size || value > border_max_size)
                        return;

                    borderSize = value;

                    InvalidateTexture();
                }
            }

            private float calculatedBorderPortion => BorderSize * border_portion;

            protected override Color4 ColourAt(float position)
            {
                if (calculatedBorderPortion != 0f && position <= calculatedBorderPortion)
                    return BorderColour;

                position -= calculatedBorderPortion;
                return new Color4(AccentColour.R, AccentColour.G, AccentColour.B, (opacity_at_edge - (opacity_at_edge - opacity_at_centre) * position / gradient_portion) * AccentColour.A);
            }
        }
    }
}
