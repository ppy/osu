// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelSetBackground : CompositeDrawable
    {
        private Sprite? sprite;

        private WorkingBeatmap? working;

        public WorkingBeatmap? Beatmap
        {
            get => working;
            set
            {
                working = value;
                loadNextBackground();
            }
        }

        public PanelSetBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Depth = -1,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                    Shear = new Vector2(0.8f, 0),
                    Children = new[]
                    {
                        // The left half with no gradient applied
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                            Width = 0.4f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0.3f)),
                            Width = 0.2f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.2f)),
                            // Slightly more than 1.0 in total to account for shear.
                            Width = 0.45f,
                        },
                    }
                },
            };
        }

        private void loadNextBackground()
        {
            const double transition_duration = 500;

            var texture = working?.GetPanelBackground();

            if (texture == null)
            {
                sprite?.FadeOut(transition_duration, Easing.OutQuint);
                sprite = null;
                return;
            }

            LoadComponentAsync(new Sprite
            {
                Depth = float.MaxValue,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                Texture = texture,
            }, s =>
            {
                sprite?.Delay(transition_duration)
                      .FadeOut();

                AddInternal(sprite = s);
                sprite.FadeInFromZero(transition_duration, Easing.OutQuint);
            });
        }
    }
}
