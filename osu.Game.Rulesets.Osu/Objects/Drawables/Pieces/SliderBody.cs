// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBody : Container, ISliderProgress
    {
        private readonly SliderPath path;
        private readonly BufferedContainer container;

        public float PathWidth
        {
            get => path.PathWidth;
            set => path.PathWidth = value;
        }

        /// <summary>
        /// Offset in absolute coordinates from the start of the curve.
        /// </summary>
        public Vector2 PathOffset { get; private set; }

        public readonly List<Vector2> CurrentCurve = new List<Vector2>();

        public readonly Bindable<bool> SnakingIn = new Bindable<bool>();
        public readonly Bindable<bool> SnakingOut = new Bindable<bool>();

        public double? SnakedStart { get; private set; }
        public double? SnakedEnd { get; private set; }

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

        private Vector2 topLeftOffset;

        private readonly Slider slider;

        public SliderBody(Slider s)
        {
            slider = s;

            Children = new Drawable[]
            {
                container = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Children = new Drawable[]
                    {
                        path = new SliderPath
                        {
                            Blending = BlendingMode.None,
                        },
                    }
                },
            };

            container.Attach(RenderbufferInternalFormat.DepthComponent16);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => path.ReceivePositionalInputAt(screenSpacePos);

        public void SetRange(double p0, double p1)
        {
            if (p0 > p1)
                MathHelper.Swap(ref p0, ref p1);

            if (updateSnaking(p0, p1))
            {
                // The path is generated such that its size encloses it. This change of size causes the path
                // to move around while snaking, so we need to offset it to make sure it maintains the
                // same position as when it is fully snaked.
                var newTopLeftOffset = path.PositionInBoundingBox(Vector2.Zero);
                path.Position = topLeftOffset - newTopLeftOffset;

                container.ForceRedraw();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            computeSize();
        }

        private void computeSize()
        {
            // Generate the entire curve
            slider.Curve.GetPathToProgress(CurrentCurve, 0, 1);
            foreach (Vector2 p in CurrentCurve)
                path.AddVertex(p);

            Size = path.Size;

            topLeftOffset = path.PositionInBoundingBox(Vector2.Zero);
            PathOffset = path.PositionInBoundingBox(CurrentCurve[0]);
        }

        private bool updateSnaking(double p0, double p1)
        {
            if (SnakedStart == p0 && SnakedEnd == p1) return false;

            SnakedStart = p0;
            SnakedEnd = p1;

            slider.Curve.GetPathToProgress(CurrentCurve, p0, p1);

            path.ClearVertices();
            foreach (Vector2 p in CurrentCurve)
                path.AddVertex(p);

            return true;
        }

        public void UpdateProgress(double completionProgress)
        {
            var span = slider.SpanAt(completionProgress);
            var spanProgress = slider.ProgressAt(completionProgress);

            double start = 0;
            double end = SnakingIn ? MathHelper.Clamp((Time.Current - (slider.StartTime - slider.TimePreempt)) / slider.TimeFadeIn, 0, 1) : 1;

            if (span >= slider.SpanCount() - 1)
            {
                if (Math.Min(span, slider.SpanCount() - 1) % 2 == 1)
                {
                    start = 0;
                    end = SnakingOut ? spanProgress : 1;
                }
                else
                {
                    start = SnakingOut ? spanProgress : 0;
                }
            }

            SetRange(start, end);
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
