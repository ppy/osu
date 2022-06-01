// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing
{
    public class TapTimingControl : CompositeDrawable
    {
        [Resolved]
        private EditorClock editorClock { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private Bindable<ControlPointGroup> selectedGroup { get; set; }

        private readonly BindableBool isHandlingTapping = new BindableBool();

        private MetronomeDisplay metronome;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            CornerRadius = LabelledDrawable<Drawable>.CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 200),
                        new Dimension(GridSizeMode.Absolute, 60),
                        new Dimension(GridSizeMode.Absolute, 60),
                        new Dimension(GridSizeMode.Absolute, 120),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension()
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                metronome = new MetronomeDisplay
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                                new WaveformComparisonDisplay(),
                                            }
                                        },
                                    }
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(10),
                                Children = new Drawable[]
                                {
                                    new TimingAdjustButton(1)
                                    {
                                        Text = "Offset",
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.48f,
                                        Height = 50,
                                        Action = adjustOffset,
                                    },
                                    new TimingAdjustButton(0.1)
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Text = "BPM",
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.48f,
                                        Height = 50,
                                        Action = adjustBpm,
                                    }
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(10),
                                Children = new Drawable[]
                                {
                                    new RoundedButton
                                    {
                                        Text = "Reset",
                                        BackgroundColour = colours.Pink,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.3f,
                                        Action = reset,
                                    },
                                    new RoundedButton
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Text = "Play from start",
                                        RelativeSizeAxes = Axes.X,
                                        BackgroundColour = colourProvider.Background1,
                                        Width = 0.68f,
                                        Action = tap,
                                    }
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new TapButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                IsHandlingTapping = { BindTarget = isHandlingTapping }
                            }
                        }
                    }
                },
            };

            isHandlingTapping.BindValueChanged(handling =>
            {
                metronome.EnableClicking = !handling.NewValue;

                if (handling.NewValue)
                {
                    editorClock.Seek(selectedGroup.Value.Time);
                    editorClock.Start();
                }
            }, true);
        }

        private void adjustOffset(double adjust)
        {
            // VERY TEMPORARY
            var currentGroupItems = selectedGroup.Value.ControlPoints.ToArray();

            beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

            double newOffset = selectedGroup.Value.Time + adjust;

            foreach (var cp in currentGroupItems)
                beatmap.ControlPointInfo.Add(newOffset, cp);

            // the control point might not necessarily exist yet, if currentGroupItems was empty.
            selectedGroup.Value = beatmap.ControlPointInfo.GroupAt(newOffset, true);

            if (!editorClock.IsRunning)
                editorClock.Seek(newOffset);
        }

        private void adjustBpm(double adjust)
        {
            var timing = selectedGroup.Value.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

            if (timing == null)
                return;

            timing.BeatLength = 60000 / (timing.BPM + adjust);
        }

        private void tap()
        {
            editorClock.Seek(selectedGroup.Value.Time);
            editorClock.Start();
        }

        private void reset()
        {
            editorClock.Stop();
            editorClock.Seek(selectedGroup.Value.Time);
        }
    }
}
