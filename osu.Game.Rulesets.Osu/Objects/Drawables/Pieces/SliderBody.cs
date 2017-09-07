// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using OpenTK;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics;

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

        public double? SnakedStart { get; private set; }
        public double? SnakedEnd { get; private set; }

        private Color4 accentColour;
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

                if (LoadState == LoadState.Ready)
                    Schedule(reloadTexture);
            }
        }

        private int textureWidth => (int)PathWidth * 2;

        private readonly Slider slider;
        public SliderBody(Slider s)
        {
            slider = s;

            Children = new Drawable[]
            {
                container = new BufferedContainer
                {
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

        public void SetRange(double p0, double p1)
        {
            if (p0 > p1)
                MathHelper.Swap(ref p0, ref p1);

            if (updateSnaking(p0, p1))
            {
                // Autosizing does not give us the desired behaviour here.
                // We want the container to have the same size as the slider,
                // and to be positioned such that the slider head is at (0,0).
                container.Size = path.Size;
                container.Position = -path.PositionInBoundingBox(slider.Curve.PositionAt(0) - currentCurve[0]);

                container.ForceRedraw();
            }
        }

        private Bindable<bool> snakingIn;
        private Bindable<bool> snakingOut;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            snakingIn = config.GetBindable<bool>(OsuSetting.SnakingInSliders);
            snakingOut = config.GetBindable<bool>(OsuSetting.SnakingOutSliders);

            reloadTexture();
        }

        private void reloadTexture()
        {
            var texture = new Texture(textureWidth, 1);

            //initialise background
            var upload = new TextureUpload(textureWidth * 4);
            var bytes = upload.Data;

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
                    bytes[i * 4] = 255;
                    bytes[i * 4 + 1] = 255;
                    bytes[i * 4 + 2] = 255;
                    bytes[i * 4 + 3] = (byte)(Math.Min(progress / aa_portion, 1) * 255);
                }
                else
                {
                    progress -= border_portion;

                    bytes[i * 4] = (byte)(AccentColour.R * 255);
                    bytes[i * 4 + 1] = (byte)(AccentColour.G * 255);
                    bytes[i * 4 + 2] = (byte)(AccentColour.B * 255);
                    bytes[i * 4 + 3] = (byte)((opacity_at_edge - (opacity_at_edge - opacity_at_centre) * progress / gradient_portion) * (AccentColour.A * 255));
                }
            }

            texture.SetData(upload);
            path.Texture = texture;
        }

        private readonly List<Vector2> currentCurve = new List<Vector2>();
        private bool updateSnaking(double p0, double p1)
        {
            if (SnakedStart == p0 && SnakedEnd == p1) return false;

            SnakedStart = p0;
            SnakedEnd = p1;

            slider.Curve.GetPathToProgress(currentCurve, p0, p1);

            path.ClearVertices();
            foreach (Vector2 p in currentCurve)
                path.AddVertex(p - currentCurve[0]);

            return true;
        }

        public void UpdateProgress(double progress, int repeat)
        {
            double start = 0;
            double end = snakingIn ? MathHelper.Clamp((Time.Current - (slider.StartTime - DrawableOsuHitObject.TIME_PREEMPT)) / DrawableOsuHitObject.TIME_FADEIN, 0, 1) : 1;

            if (repeat >= slider.RepeatCount - 1)
            {
                if (Math.Min(repeat, slider.RepeatCount - 1) % 2 == 1)
                {
                    start = 0;
                    end = snakingOut ? progress : 1;
                }
                else
                {
                    start = snakingOut ? progress : 0;
                }
            }

            SetRange(start, end);
        }
    }
}