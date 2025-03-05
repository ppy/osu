// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanelBackground : ModelBackedDrawable<WorkingBeatmap>
    {
        protected override double TransformDuration => 400;

        public WorkingBeatmap? Beatmap
        {
            get => Model;
            set => Model = value;
        }

        protected override Drawable CreateDrawable(WorkingBeatmap? model) => new BackgroundSprite(model);

        private partial class BackgroundSprite : CompositeDrawable
        {
            private readonly WorkingBeatmap? working;

            public BackgroundSprite(WorkingBeatmap? working)
            {
                this.working = working;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                var texture = working?.GetPanelBackground();

                if (texture != null)
                {
                    InternalChildren = new Drawable[]
                    {
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                            Texture = texture,
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
                else
                {
                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background6,
                    };
                }
            }
        }
    }
}
