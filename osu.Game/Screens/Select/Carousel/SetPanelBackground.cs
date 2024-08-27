// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class SetPanelBackground : BufferedContainer
    {
        public SetPanelBackground(IWorkingBeatmap working)
        {
            RedrawOnScale = false;

            Children = new Drawable[]
            {
                background = new PanelBeatmapBackground(working)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                },
                new FillFlowContainer
                {
                    Depth = -1,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                    Shear = new Vector2(0.8f, 0),
                    Alpha = 0.5f,
                    Children = new[]
                    {
                        // The left half with no gradient applied
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Width = 0.4f,
                        },
                        // Piecewise-linear gradient with 3 segments to make it appear smoother
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black, new Color4(0f, 0f, 0f, 0.9f)),
                            Width = 0.05f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.9f), new Color4(0f, 0f, 0f, 0.1f)),
                            Width = 0.2f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(new Color4(0f, 0f, 0f, 0.1f), new Color4(0, 0, 0, 0)),
                            Width = 0.05f,
                        },
                    }
                },
            };
        }

        [Resolved(canBeNull: true)]
        private DrawableCarouselBeatmapSet? carouselBeatmapSet { get; set; }

        private readonly PanelBeatmapBackground background;

        private const float parallax_strength = 35;

        protected override void Update()
        {
            base.Update();

            if (carouselBeatmapSet != null)
            {
                float maxParallax = (background.DrawHeight - DrawHeight) / 2f;

                background.Y = Math.Clamp(carouselBeatmapSet.Parallax, -1f, 1f) * Math.Min(maxParallax, parallax_strength);
            }
        }

        public partial class PanelBeatmapBackground : Sprite
        {
            private readonly IWorkingBeatmap working;

            public PanelBeatmapBackground(IWorkingBeatmap working)
            {
                ArgumentNullException.ThrowIfNull(working);

                this.working = working;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Texture = working.GetPanelBackground();
            }
        }
    }
}
