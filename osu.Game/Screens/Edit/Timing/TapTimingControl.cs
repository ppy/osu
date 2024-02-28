// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class TapTimingControl : CompositeDrawable
    {
        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup> selectedGroup { get; set; } = null!;

        private readonly BindableBool isHandlingTapping = new BindableBool();

        private MetronomeDisplay metronome = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            const float padding = 10;

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
                        new Dimension(GridSizeMode.Absolute, 50),
                        new Dimension(GridSizeMode.Absolute, TapButton.SIZE + padding),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(padding),
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
                                        new WaveformComparisonDisplay()
                                    }
                                },
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = padding, Horizontal = padding },
                                Children = new Drawable[]
                                {
                                    new TimingAdjustButton(1)
                                    {
                                        Text = "Offset",
                                        RelativeSizeAxes = Axes.Both,
                                        Size = new Vector2(0.48f, 1),
                                        Action = adjustOffset,
                                    },
                                    new TimingAdjustButton(0.1)
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Text = "BPM",
                                        RelativeSizeAxes = Axes.Both,
                                        Size = new Vector2(0.48f, 1),
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
                                Padding = new MarginPadding { Bottom = padding, Horizontal = padding },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.CentreRight,
                                        Height = 0.98f,
                                        Width = TapButton.SIZE / 1.3f,
                                        Masking = true,
                                        CornerRadius = 15,
                                        Children = new Drawable[]
                                        {
                                            new InlineButton(FontAwesome.Solid.Stop, Anchor.TopLeft)
                                            {
                                                BackgroundColour = colourProvider.Background1,
                                                RelativeSizeAxes = Axes.Both,
                                                Height = 0.49f,
                                                Action = reset,
                                            },
                                            new InlineButton(FontAwesome.Solid.Play, Anchor.BottomLeft)
                                            {
                                                BackgroundColour = colourProvider.Background1,
                                                RelativeSizeAxes = Axes.Both,
                                                Height = 0.49f,
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Action = start,
                                            },
                                        },
                                    },
                                    new TapButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        IsHandlingTapping = { BindTarget = isHandlingTapping }
                                    }
                                }
                            },
                        },
                    }
                },
            };

            isHandlingTapping.BindValueChanged(handling =>
            {
                metronome.EnableClicking = !handling.NewValue;

                if (handling.NewValue)
                    start();
            }, true);
        }

        private void start()
        {
            if (selectedGroup.Value == null)
                return;

            editorClock.Seek(selectedGroup.Value.Time);
            editorClock.Start();
        }

        private void reset()
        {
            if (selectedGroup.Value == null)
                return;

            editorClock.Stop();
            editorClock.Seek(selectedGroup.Value.Time);
        }

        private void adjustOffset(double adjust)
        {
            if (selectedGroup.Value == null)
                return;

            bool wasAtStart = editorClock.CurrentTimeAccurate == selectedGroup.Value.Time;

            // VERY TEMPORARY
            var currentGroupItems = selectedGroup.Value.ControlPoints.ToArray();

            beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

            double newOffset = selectedGroup.Value.Time + adjust;

            foreach (var cp in currentGroupItems)
                beatmap.ControlPointInfo.Add(newOffset, cp);

            // the control point might not necessarily exist yet, if currentGroupItems was empty.
            selectedGroup.Value = beatmap.ControlPointInfo.GroupAt(newOffset, true);

            if (!editorClock.IsRunning && wasAtStart)
                editorClock.Seek(newOffset);
        }

        private void adjustBpm(double adjust)
        {
            var timing = selectedGroup.Value?.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

            if (timing == null)
                return;

            timing.BeatLength = 60000 / (timing.BPM + adjust);
        }

        private partial class InlineButton : OsuButton
        {
            private readonly IconUsage icon;
            private readonly Anchor anchor;

            private SpriteIcon spriteIcon = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public InlineButton(IconUsage icon, Anchor anchor)
            {
                this.icon = icon;
                this.anchor = anchor;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Content.CornerRadius = 0;
                Content.Masking = false;

                BackgroundColour = colourProvider.Background2;

                Content.Add(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(15),
                    Children = new Drawable[]
                    {
                        spriteIcon = new SpriteIcon
                        {
                            Icon = icon,
                            Size = new Vector2(22),
                            Anchor = anchor,
                            Origin = anchor,
                            Colour = colourProvider.Background1,
                        },
                    }
                });
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                // scale looks bad so don't call base.
                return false;
            }

            protected override bool OnHover(HoverEvent e)
            {
                spriteIcon.FadeColour(colourProvider.Content2, 200, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                spriteIcon.FadeColour(colourProvider.Background1, 200, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
