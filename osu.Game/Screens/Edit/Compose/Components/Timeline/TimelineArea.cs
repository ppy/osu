// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineArea : CompositeDrawable
    {
        public Timeline Timeline = null!;

        private readonly Drawable userContent;

        public TimelineArea(Drawable? content = null)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            userContent = content ?? Empty();
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
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 140),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 30),
                        new Dimension(GridSizeMode.Absolute, 110),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
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
                                                LabelText = EditorStrings.TimelineWaveform,
                                                Current = { Value = true },
                                            },
                                            ticksCheckbox = new OsuCheckbox
                                            {
                                                LabelText = EditorStrings.TimelineTicks,
                                                Current = { Value = true },
                                            },
                                            controlPointsCheckbox = new OsuCheckbox
                                            {
                                                LabelText = BeatmapsetsStrings.ShowStatsBpm,
                                                Current = { Value = true },
                                            },
                                        }
                                    }
                                }
                            },
                            Timeline = new Timeline(userContent),
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Name = @"Zoom controls",
                                Padding = new MarginPadding { Right = 5 },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background2,
                                    },
                                    new Container<TimelineButton>
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        Masking = true,
                                        Children = new[]
                                        {
                                            new TimelineButton
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Size = new Vector2(1, 0.5f),
                                                Icon = FontAwesome.Solid.SearchPlus,
                                                Action = () => Timeline.AdjustZoomRelatively(1)
                                            },
                                            new TimelineButton
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                RelativeSizeAxes = Axes.Both,
                                                Size = new Vector2(1, 0.5f),
                                                Icon = FontAwesome.Solid.SearchMinus,
                                                Action = () => Timeline.AdjustZoomRelatively(-1)
                                            },
                                        }
                                    }
                                }
                            },
                            new BeatDivisorControl { RelativeSizeAxes = Axes.Both }
                        },
                    },
                }
            };

            Timeline.WaveformVisible.BindTo(waveformCheckbox.Current);
            Timeline.ControlPointsVisible.BindTo(controlPointsCheckbox.Current);
            Timeline.TicksVisible.BindTo(ticksCheckbox.Current);
        }
    }
}
