// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class MetronomeDisplay : BeatSyncedContainer
    {
        private Container swing = null!;

        private OsuSpriteText bpmText = null!;

        private Drawable weight = null!;
        private Drawable stick = null!;

        private IAdjustableClock metronomeClock = null!;

        private Sample? sampleTick;
        private Sample? sampleTickDownbeat;
        private Sample? sampleLatch;

        private ScheduledDelegate? tickPlaybackDelegate;

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public bool EnableClicking { get; set; } = true;

        public MetronomeDisplay()
        {
            AllowMistimedEventFiring = false;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleTick = audio.Samples.Get(@"UI/metronome-tick");
            sampleTickDownbeat = audio.Samples.Get(@"UI/metronome-tick-downbeat");
            sampleLatch = audio.Samples.Get(@"UI/metronome-latch");

            const float taper = 25;
            const float swing_vertical_offset = -23;
            const float lower_cover_height = 32;

            var triangleSize = new Vector2(90, 120 + taper);

            Margin = new MarginPadding(10);

            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = @"Taper adjust",
                    Masking = true,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangleSize.X, triangleSize.Y - taper),
                    Children = new Drawable[]
                    {
                        new Triangle
                        {
                            Name = @"Main body",
                            EdgeSmoothness = new Vector2(1),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Size = triangleSize,
                            Colour = overlayColourProvider.Background3,
                        },
                    },
                },
                new Circle
                {
                    Name = "Centre marker",
                    Colour = overlayColourProvider.Background5,
                    RelativeSizeAxes = Axes.Y,
                    Width = 2,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -(lower_cover_height + 3),
                    Height = 0.65f,
                },
                swing = new Container
                {
                    Name = @"Swing",
                    RelativeSizeAxes = Axes.Both,
                    Y = swing_vertical_offset,
                    Height = 0.80f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new[]
                    {
                        stick = new Circle
                        {
                            Name = @"Stick",
                            RelativeSizeAxes = Axes.Y,
                            Colour = overlayColourProvider.Colour2,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Width = 4,
                        },
                        weight = new Container
                        {
                            Name = @"Weight",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(10),
                            Rotation = 180,
                            RelativePositionAxes = Axes.Y,
                            Y = 0.4f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Shear = new Vector2(0.2f, 0),
                                    Colour = overlayColourProvider.Colour1,
                                    EdgeSmoothness = new Vector2(1),
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Shear = new Vector2(-0.2f, 0),
                                    Colour = overlayColourProvider.Colour1,
                                    EdgeSmoothness = new Vector2(1),
                                },
                                new Circle
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = ColourInfo.GradientVertical(overlayColourProvider.Colour1, overlayColourProvider.Colour0),
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 1,
                                    Height = 0.9f
                                },
                            }
                        },
                    }
                },
                new Container
                {
                    Name = @"Taper adjust",
                    Masking = true,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangleSize.X, triangleSize.Y - taper),
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Name = @"Locking wedge",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Colour = overlayColourProvider.Background1,
                            Size = new Vector2(8),
                        }
                    },
                },
                new Circle
                {
                    Name = @"Swing connection point",
                    Y = swing_vertical_offset,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Colour = overlayColourProvider.Colour0,
                    Size = new Vector2(8)
                },
                new Container
                {
                    Name = @"Lower cover",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Masking = true,
                    Height = lower_cover_height,
                    Children = new Drawable[]
                    {
                        new Triangle
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Size = triangleSize,
                            Colour = overlayColourProvider.Background2,
                            EdgeSmoothness = new Vector2(1),
                            Alpha = 0.8f
                        },
                    }
                },
                bpmText = new OsuSpriteText
                {
                    Name = @"BPM display",
                    Colour = overlayColourProvider.Content1,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -3,
                },
            };

            Clock = new FramedClock(metronomeClock = new StopwatchClock(true));
        }

        private double beatLength;

        private TimingControlPoint timingPoint = null!;

        private bool isSwinging;

        private readonly BindableInt interpolatedBpm = new BindableInt();

        private ScheduledDelegate? latchDelegate;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            interpolatedBpm.BindValueChanged(bpm => bpmText.Text = bpm.NewValue.ToLocalisableString());
        }

        protected override void Update()
        {
            base.Update();

            if (BeatSyncSource.ControlPoints == null)
                return;

            metronomeClock.Rate = IsBeatSyncedWithTrack ? BeatSyncSource.Clock.Rate : 1;

            timingPoint = BeatSyncSource.ControlPoints.TimingPointAt(BeatSyncSource.Clock.CurrentTime);

            if (beatLength != timingPoint.BeatLength)
            {
                beatLength = timingPoint.BeatLength;

                EarlyActivationMilliseconds = timingPoint.BeatLength / 2;

                float bpmRatio = (float)Interpolation.ApplyEasing(Easing.OutQuad, Math.Clamp((timingPoint.BPM - 30) / 480, 0, 1));

                weight.MoveToY((float)Interpolation.Lerp(0.1f, 0.83f, bpmRatio), 600, Easing.OutQuint);
                this.TransformBindableTo(interpolatedBpm, (int)Math.Round(timingPoint.BPM), 600, Easing.OutQuint);
            }

            if (!BeatSyncSource.Clock.IsRunning && isSwinging)
            {
                swing.ClearTransforms(true);

                isSwinging = false;

                tickPlaybackDelegate?.Cancel();
                tickPlaybackDelegate = null;

                // instantly latch if pendulum arm is close enough to center (to prevent awkward delayed playback of latch sound)
                if (Precision.AlmostEquals(swing.Rotation, 0, 1))
                {
                    swing.RotateTo(0, 60, Easing.OutQuint);
                    stick.FadeColour(overlayColourProvider.Colour2, 1000, Easing.OutQuint);
                    sampleLatch?.Play();
                    return;
                }

                using (BeginDelayedSequence(350))
                {
                    swing.RotateTo(0, 1000, Easing.OutQuint);
                    stick.FadeColour(overlayColourProvider.Colour2, 1000, Easing.OutQuint);

                    using (BeginDelayedSequence(380))
                        latchDelegate = Schedule(() => sampleLatch?.Play());
                }
            }
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            const float angle = 27.5f;

            if (!IsBeatSyncedWithTrack)
                return;

            isSwinging = true;

            latchDelegate?.Cancel();
            latchDelegate = null;

            float currentAngle = swing.Rotation;
            float targetAngle = currentAngle > 0 ? -angle : angle;

            swing.RotateTo(targetAngle, beatLength, Easing.InOutQuad);

            if (currentAngle != 0 && Math.Abs(currentAngle - targetAngle) > angle * 1.8f && isSwinging)
            {
                using (BeginDelayedSequence(beatLength / 2))
                {
                    stick.FlashColour(overlayColourProvider.Content1, beatLength, Easing.OutQuint);

                    tickPlaybackDelegate = Schedule(() =>
                    {
                        if (!EnableClicking)
                            return;

                        var channel = beatIndex % timingPoint.TimeSignature.Numerator == 0 ? sampleTickDownbeat?.GetChannel() : sampleTick?.GetChannel();

                        if (channel == null)
                            return;

                        channel.Frequency.Value = RNG.NextDouble(0.98f, 1.02f);
                        channel.Play();
                    });
                }
            }
        }
    }
}
