// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Textures;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBody : Container, ISliderProgress
    {
        private readonly Path path;
        private readonly BufferedContainer container;

        public float PathWidth
        {
            get { return path.PathWidth; }
            set { path.PathWidth = value; }
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

        private Color4 accentColour = Color4.White;

        /// <summary>
        /// Used to colour the path.
        /// </summary>
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                if (LoadState >= LoadState.Ready)
                    reloadTexture();
            }
        }

        private Color4 borderColour = Color4.White;

        /// <summary>
        /// Used to colour the path border.
        /// </summary>
        public new Color4 BorderColour
        {
            get { return borderColour; }
            set
            {
                if (borderColour == value)
                    return;
                borderColour = value;

                if (LoadState >= LoadState.Ready)
                    reloadTexture();
            }
        }

        public Quad PathDrawQuad => container.ScreenSpaceDrawQuad;

        private int textureWidth => (int)PathWidth * 2;

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
                        path = new Path
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
            reloadTexture();
            computeSize();
        }

        private void reloadTexture()
        {
            var texture = new Texture(textureWidth, 1);

            //initialise background
            var raw = new Image<Rgba32>(textureWidth, 1);

            const float aa_portion = 0.02f;
            const float border_portion = 0.128f;
            const float gradient_portion = 1 - border_portion;

            const float opacity_at_centre = 0.3f;
            const float opacity_at_edge = 0.8f;

            for (int i = 0; i < textureWidth; i++)
            {
                float progress = (float)i / (textureWidth - 1);

                if (progress <= border_portion)
                {
                    raw[i, 0] = new Rgba32(BorderColour.R, BorderColour.G, BorderColour.B, Math.Min(progress / aa_portion, 1) * BorderColour.A);
                }
                else
                {
                    progress -= border_portion;
                    raw[i, 0] = new Rgba32(AccentColour.R, AccentColour.G, AccentColour.B,
                        (opacity_at_edge - (opacity_at_edge - opacity_at_centre) * progress / gradient_portion) * AccentColour.A);
                }
            }

            texture.SetData(new TextureUpload(raw));
            path.Texture = texture;

            container.ForceRedraw();
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
    }
}
