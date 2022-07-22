// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineArea : CompositeDrawable
    {
        public Timeline Timeline;

        private readonly Drawable userContent;

        public TimelineArea(Drawable content = null)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            userContent = content ?? Drawable.Empty();
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;

            OsuCheckbox waveformCheckbox;
            OsuCheckbox controlPointsCheckbox;
            OsuCheckbox ticksCheckbox;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Name = @"Toggle controls",
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background2,
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        Width = 160,
                                        Padding = new MarginPadding(10),
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 4),
                                        Children = new[]
                                        {
                                            waveformCheckbox = new OsuCheckbox
                                            {
                                                LabelText = "Waveform",
                                                Current = { Value = true },
                                            },
                                            controlPointsCheckbox = new OsuCheckbox
                                            {
                                                LabelText = "Control Points",
                                                Current = { Value = true },
                                            },
                                            ticksCheckbox = new OsuCheckbox
                                            {
                                                LabelText = "Ticks",
                                                Current = { Value = true },
                                            }
                                        }
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Name = @"Zoom controls",
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background3,
                                    },
                                    new Container<TimelineButton>
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Masking = true,
                                        Children = new[]
                                        {
                                            new TimelineButton
                                            {
                                                RelativeSizeAxes = Axes.Y,
                                                Height = 0.5f,
                                                Icon = FontAwesome.Solid.SearchPlus,
                                                Action = () => Timeline.AdjustZoomRelatively(1)
                                            },
                                            new TimelineButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                RelativeSizeAxes = Axes.Y,
                                                Height = 0.5f,
                                                Icon = FontAwesome.Solid.SearchMinus,
                                                Action = () => Timeline.AdjustZoomRelatively(-1)
                                            },
                                        }
                                    }
                                }
                            },
                            Timeline = new Timeline(userContent),
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    }
                }
            };

            Timeline.WaveformVisible.BindTo(waveformCheckbox.Current);
            Timeline.ControlPointsVisible.BindTo(controlPointsCheckbox.Current);
            Timeline.TicksVisible.BindTo(ticksCheckbox.Current);
        }
    }
}
