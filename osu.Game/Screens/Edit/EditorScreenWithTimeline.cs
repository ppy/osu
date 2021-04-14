// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
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

        private Container timelineContainer;

        protected EditorScreenWithTimeline(EditorScreenMode type)
            : base(type)
        {
        }

        private Container mainContent;

        private LoadingSpinner spinner;

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] BindableBeatDivisor beatDivisor)
        {
            if (beatDivisor != null)
                this.beatDivisor.BindTo(beatDivisor);

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
                                    Colour = Color4.Black.Opacity(0.5f)
                                },
                                new Container
                                {
                                    Name = "Timeline content",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = horizontal_margins, Vertical = vertical_margins },
                                    Child = new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                timelineContainer = new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Padding = new MarginPadding { Right = 5 },
                                                },
                                                new BeatDivisorControl(beatDivisor) { RelativeSizeAxes = Axes.Both }
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
                        mainContent = new Container
                        {
                            Name = "Main content",
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MaxValue,
                            Padding = new MarginPadding
                            {
                                Horizontal = horizontal_margins,
                                Top = vertical_margins,
                                Bottom = vertical_margins
                            },
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

                mainContent.Add(content);
                content.FadeInFromZero(300, Easing.OutQuint);

                LoadComponentAsync(new TimelineArea(CreateTimelineContent()), t =>
                {
                    timelineContainer.Add(t);
                    OnTimelineLoaded(t);
                });
            });
        }

        protected virtual void OnTimelineLoaded(TimelineArea timelineArea)
        {
        }

        protected abstract Drawable CreateMainContent();

        protected virtual Drawable CreateTimelineContent() => new Container();
    }
}
