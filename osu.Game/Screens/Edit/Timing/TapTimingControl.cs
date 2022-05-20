// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

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
            private Box stick;

            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; }

            [BackgroundDependencyLoader]
            private void load()
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
                        Colour = overlayColourProvider.Background2,
                    },
                    bpm = new OsuSpriteText
                    {
                        Colour = overlayColourProvider.Content1,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
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
                            stick = new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Colour = overlayColourProvider.Colour2,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Width = 4,
                            },
                            weight = new Box
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Colour = overlayColourProvider.Colour1,
                                Size = new Vector2(15),
                                RelativePositionAxes = Axes.Y,
                                Y = 0.4f,
                            },
                        }
                    },
                    new Circle
                    {
                        Y = -25,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        Colour = overlayColourProvider.Colour0,
                        Size = new Vector2(10)
                    },
                };
            }

            private double beatLength;

            private TimingControlPoint timingPoint;

            private float bpmRatio;
            private bool isSwinging;

            protected override void Update()
            {
                base.Update();

                timingPoint = Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(BeatSyncClock.CurrentTime);

                if (beatLength != timingPoint.BeatLength)
                {
                    beatLength = timingPoint.BeatLength;
                    bpm.Text = $"{timingPoint.BPM:F0}";

                    EarlyActivationMilliseconds = timingPoint.BeatLength / 2;

                    bpmRatio = (float)Interpolation.ApplyEasing(Easing.OutQuad, Math.Clamp((timingPoint.BPM - 30) / 480, 0, 1));

                    weight.MoveToY((float)Interpolation.Lerp(0, 0.9f, bpmRatio), 600, Easing.OutQuint);
                }

                if (BeatSyncClock?.IsRunning != true && isSwinging)
                {
                    swing.ClearTransforms(true);

                    using (swing.BeginDelayedSequence(350))
                    {
                        swing.RotateTo(0, 1000, Easing.OutQuint);
                        stick.FadeColour(overlayColourProvider.Colour2, 1000, Easing.OutQuint);
                    }

                    isSwinging = false;
                }
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                float angle = (float)Interpolation.Lerp(25, 4, bpmRatio);

                if (!IsBeatSyncedWithTrack)
                    return;

                isSwinging = true;

                float currentAngle = swing.Rotation;
                float targetAngle = currentAngle > 0 ? -angle : angle;

                swing.RotateTo(targetAngle, beatLength, Easing.InOutQuad);

                if (currentAngle != 0 && Math.Abs(currentAngle - targetAngle) > angle * 1.8f && isSwinging)
                {
                    using (stick.BeginDelayedSequence(beatLength / 2))
                        stick.FlashColour(overlayColourProvider.Content1, beatLength, Easing.OutQuint);
                }
            }
        }
    }
}
