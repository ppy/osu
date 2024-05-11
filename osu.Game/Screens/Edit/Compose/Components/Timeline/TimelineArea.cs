// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Edit;
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
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            OsuCheckbox waveformCheckbox;
            OsuCheckbox controlPointsCheckbox;
            OsuCheckbox ticksCheckbox;

            const float padding = 10;

            InternalChildren = new Drawable[]
            {
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
                        new Dimension(GridSizeMode.Absolute, 135),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 35),
                        new Dimension(GridSizeMode.Absolute, HitObjectComposer.TOOLBOX_CONTRACTED_SIZE_RIGHT - padding * 2),
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
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding(padding),
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 4),
                                        Children = new[]
                                        {
                                            waveformCheckbox = new OsuCheckbox(nubSize: 30f)
                                            {
                                                LabelText = EditorStrings.TimelineWaveform,
                                                Current = { Value = true },
                                            },
                                            ticksCheckbox = new OsuCheckbox(nubSize: 30f)
                                            {
                                                LabelText = EditorStrings.TimelineTicks,
                                                Current = { Value = true },
                                            },
                                            controlPointsCheckbox = new OsuCheckbox(nubSize: 30f)
                                            {
                                                LabelText = BeatmapsetsStrings.ShowStatsBpm,
                                                Current = { Value = true },
                                            },
                                        }
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    // the out-of-bounds portion of the centre marker.
                                    new Box
                                    {
                                        Width = 24,
                                        Height = EditorScreenWithTimeline.PADDING,
                                        Depth = float.MaxValue,
                                        Colour = colours.Red1,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.BottomCentre,
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Depth = float.MaxValue,
                                        Colour = colourProvider.Background5
                                    },
                                    Timeline = new Timeline(userContent),
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Name = @"Zoom controls",
                                Padding = new MarginPadding { Right = padding },
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
