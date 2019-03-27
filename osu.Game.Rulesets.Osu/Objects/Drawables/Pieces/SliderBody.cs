// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public abstract class SliderBody : CompositeDrawable
    {
        private readonly SliderPath path;
        protected Path Path => path;

        private readonly BufferedContainer container;

        public float PathRadius
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

                container.ForceRedraw();
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

                container.ForceRedraw();
            }
        }

        public Quad PathDrawQuad => container.ScreenSpaceDrawQuad;

        protected SliderBody()
        {
            InternalChild = container = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                CacheDrawnFrameBuffer = true,
                Child = path = new SliderPath { Blending = BlendingMode.None }
            };

            container.Attach(RenderbufferInternalFormat.DepthComponent16);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => path.ReceivePositionalInputAt(screenSpacePos);

        /// <summary>
        /// Sets the vertices of the path which should be drawn by this <see cref="SliderBody"/>.
        /// </summary>
        /// <param name="vertices">The vertices</param>
        protected void SetVertices(IReadOnlyList<Vector2> vertices)
        {
            path.Vertices = vertices;
            container.ForceRedraw();
        }

        private class SliderPath : SmoothPath
        {
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

            protected override Color4 ColourAt(float position)
            {
                if (position <= border_portion)
                    return BorderColour;

                position -= border_portion;
                return new Color4(AccentColour.R, AccentColour.G, AccentColour.B, (opacity_at_edge - (opacity_at_edge - opacity_at_centre) * position / gradient_portion) * AccentColour.A);
            }
        }
    }
}
