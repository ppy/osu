// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TapTimingControl : CompositeDrawable
    {
        [Resolved]
        private EditorClock editorClock { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            Height = 200;
            RelativeSizeAxes = Axes.X;

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
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 60),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Metronome()
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
                                        Text = "Tap to beat",
                                        RelativeSizeAxes = Axes.X,
                                        BackgroundColour = colourProvider.Background1,
                                        Width = 0.68f,
                                        Action = tap,
                                    }
                                }
                            },
                        }
                    }
                },
            };
        }

        private void tap()
        {
            if (!editorClock.IsRunning)
            {
                editorClock.Seek(0);
                editorClock.Start();
            }
        }

        private void reset()
        {
            editorClock.Stop();
        }

        private class Metronome : BeatSyncedContainer
        {
            private Container swing;
            private Box weight;
            private OsuSpriteText bpm;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider overlayColourProvider)
            {
                Margin = new MarginPadding(10);
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Triangle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(80, 120),
                        Colour = overlayColourProvider.Background1,
                    },
                    new Circle
                    {
                        Y = -25,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        Colour = overlayColourProvider.Content2,
                        Size = new Vector2(10)
                    },
                    bpm = new OsuSpriteText
                    {
                        Colour = overlayColourProvider.Content1,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    swing = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Y = -25,
                        Height = 0.8f,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = overlayColourProvider.Content2,
                                RelativeSizeAxes = Axes.Y,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Width = 4,
                            },
                            weight = new Box
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Colour = overlayColourProvider.Content2,
                                Size = new Vector2(15),
                                RelativePositionAxes = Axes.Y,
                                Y = -0.4f,
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                swing
                    .RotateTo(20, 500, Easing.InOutQuad)
                    .Then()
                    .RotateTo(-20, 500, Easing.InOutQuad)
                    .Loop();
            }

            protected override void Update()
            {
                base.Update();

                if (CurrentTimingPoint == null)
                    return;

                weight.Y = Math.Clamp((float)CurrentTimingPoint.BPM / 480, 0, 0.95f);
                bpm.Text = $"{CurrentTimingPoint.BPM:F0}";
            }
        }
    }
}
