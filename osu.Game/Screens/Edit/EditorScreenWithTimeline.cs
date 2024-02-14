// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit
{
    [Cached]
    public abstract partial class EditorScreenWithTimeline : EditorScreen
    {
        public const float PADDING = 10;

        public Container TimelineContent { get; private set; } = null!;

        public Container MainContent { get; private set; } = null!;

        private LoadingSpinner spinner = null!;

        protected EditorScreenWithTimeline(EditorScreenMode type)
            : base(type)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider colourProvider)
        {
            // Grid with only two rows.
            // First is the timeline area, which should be allowed to expand as required.
            // Second is the main editor content, including the playfield and side toolbars (but not the bottom).
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            Name = "Timeline",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background4
                                },
                                new Container
                                {
                                    Name = "Timeline content",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = PADDING, Top = PADDING },
                                    Child = new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                TimelineContent = new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                },
                                            },
                                        },
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 90),
                                        }
                                    },
                                }
                            }
                        },
                    },
                    new Drawable[]
                    {
                        MainContent = new Container
                        {
                            Name = "Main content",
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MaxValue,
                            Child = spinner = new LoadingSpinner(true)
                            {
                                State = { Value = Visibility.Visible },
                            },
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(CreateMainContent(), content =>
            {
                spinner.State.Value = Visibility.Hidden;

                MainContent.Add(content);
                content.FadeInFromZero(300, Easing.OutQuint);

                LoadComponentAsync(new TimelineArea(CreateTimelineContent()), TimelineContent.Add);
            });
        }

        protected abstract Drawable CreateMainContent();

        protected virtual Drawable CreateTimelineContent() => new Container();
    }
}
