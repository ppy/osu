// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public abstract class EditorScreenWithTimeline : EditorScreen
    {
        private const float vertical_margins = 10;
        private const float horizontal_margins = 20;

        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] BindableBeatDivisor beatDivisor)
        {
            if (beatDivisor != null)
                this.beatDivisor.BindTo(beatDivisor);

            Container mainContent;

            Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "Timeline",
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black.Opacity(0.5f)
                                    },
                                    new Container
                                    {
                                        Name = "Timeline content",
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Horizontal = horizontal_margins, Vertical = vertical_margins },
                                        Child = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Padding = new MarginPadding { Right = 5 },
                                                        Child = CreateTimeline()
                                                    },
                                                    new BeatDivisorControl(beatDivisor) { RelativeSizeAxes = Axes.Both }
                                                },
                                            },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 90),
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            mainContent = new Container
                            {
                                Name = "Main content",
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = horizontal_margins, Vertical = vertical_margins },
                            }
                        }
                    },
                    RowDimensions = new[] { new Dimension(GridSizeMode.Absolute, 110) }
                },
            };

            LoadComponentAsync(CreateMainContent(), content =>
            {
                mainContent.Add(content);
                content.FadeInFromZero(300, Easing.OutQuint);
            });
        }

        protected abstract Drawable CreateMainContent();

        protected virtual Drawable CreateTimeline() => new TimelineArea { RelativeSizeAxes = Axes.Both };
    }
}
