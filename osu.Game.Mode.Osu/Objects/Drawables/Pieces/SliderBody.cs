//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SliderBody : Container, ISliderProgress
    {
        private Path path;
        private BodyTexture pathTexture;
        private BufferedContainer container;

        public float PathWidth
        {
            get { return path.PathWidth; }
            set
            {
                path.PathWidth = value;
                pathTexture.Size = new Vector2(value, 1);
            }
        }

        private double? drawnProgressStart;
        private double? drawnProgressEnd;

        private Slider slider;
        public SliderBody(Slider s)
        {
            slider = s;

            Children = new Drawable[]
            {
                pathTexture = new BodyTexture
                {
                    MainColour = s.Colour
                },
                container = new BufferedContainer
                {
                    CacheDrawnFrameBuffer = true,
                    Children = new Drawable[]
                    {
                        path = new Path
                        {
                            BlendingMode = BlendingMode.None,
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
            snakingIn = config.GetBindable<bool>(OsuConfig.SnakingInSliders);
            snakingOut = config.GetBindable<bool>(OsuConfig.SnakingOutSliders);
        }

        public double SnakedAmount { get; private set; }

        private List<Vector2> currentCurve = new List<Vector2>();
        private bool updateSnaking(double p0, double p1)
        {
            if (drawnProgressStart == p0 && drawnProgressEnd == p1) return false;

            drawnProgressStart = p0;
            drawnProgressEnd = p1;

            slider.Curve.GetPathToProgress(currentCurve, p0, p1);

            path.ClearVertices();
            foreach (Vector2 p in currentCurve)
                path.AddVertex(p - currentCurve[0]);

            return true;
        }

        protected override void Update()
        {
            base.Update();

            var texture = pathTexture.Texture;
            if (texture != null)
            {
                path.Texture = texture;
                pathTexture.Alpha = 0;
            }
        }

        public void UpdateProgress(double progress, int repeat)
        {
            double start = 0;
            double end = SnakedAmount = snakingIn ? MathHelper.Clamp((Time.Current - (slider.StartTime - DrawableOsuHitObject.TIME_PREEMPT)) / DrawableOsuHitObject.TIME_FADEIN, 0, 1) : 1;

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

        public class BodyTexture : BufferedContainer
        {
            private Box gradientPortion;
            private BufferedContainerDrawNode lastNode;
            public Texture Texture => lastNode?.FrameBuffers[0].Texture != null ? new Texture(lastNode.FrameBuffers[0].Texture) : null;

            protected override void ApplyDrawNode(DrawNode node)
            {
                base.ApplyDrawNode(node);
                lastNode = node as BufferedContainerDrawNode;
            }

            public Color4 MainColour
            {
                set
                {
                    gradientPortion.ColourInfo = ColourInfo.GradientHorizontal(
                        value,
                        new Color4(value.R, value.G, value.B, value.A * 0.4f)
                    );
                }
            }

            public BodyTexture()
            {
                Children = new[]
                {
                    new FlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FlowDirection.HorizontalOnly,
                        Masking = true,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColourInfo = ColourInfo.GradientHorizontal(
                                    new Color4(255, 255, 255, 0),
                                    Color4.White
                                ),
                                Size = new Vector2(0.02f, 1),
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                                Size = new Vector2(0.16f, 1),
                            },
                            gradientPortion = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.82f, 1),
                            },
                        },
                    }
                };
            }
        }
    }
}