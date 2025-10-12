// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Globalization;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Audio.DecibelScaling;

namespace osu.Game.Overlays.Volume
{
    public partial class VolumeMeter : Container, IStateful<SelectionState>
    {
        private CircularProgress volumeCircle;
        private CircularProgress volumeCircleGlow;

        protected static readonly Vector2 LABEL_SIZE = new Vector2(120, 20);

        public BindableDouble Bindable { get; } = new BindableDouble { MinValue = 0, MaxValue = 1 };

        protected readonly float CircleSize;

        private readonly Color4 meterColour;
        private readonly string name;

        private OsuSpriteText text;
        private BufferedContainer maxGlow;

        private Container selectedGlowContainer;

        private Sample hoverSample;
        private Sample notchSample;
        private double sampleLastPlaybackTime;

        [CanBeNull]
        public event Action<SelectionState> StateChanged;

        private SelectionState state;

        public SelectionState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;
                StateChanged?.Invoke(value);

                updateSelectedState();
            }
        }

        private const float transition_length = 500;

        public VolumeMeter(string name, float circleSize, Color4 meterColour)
        {
            CircleSize = circleSize;
            this.meterColour = meterColour;
            this.name = name;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            hoverSample = audio.Samples.Get($@"UI/{HoverSampleSet.Button.GetDescription()}-hover");
            notchSample = audio.Samples.Get(@"UI/notch-tick");
            sampleLastPlaybackTime = Time.Current;

            Color4 backgroundColour = colours.Gray1;

            CircularProgress bgProgress;

            const float progress_start_radius = 0.75f;
            const float progress_size = 0.03f;
            const float progress_end_radius = progress_start_radius + progress_size;

            const float blur_amount = 5;

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(CircleSize),
                    Children = new Drawable[]
                    {
                        new BufferedContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = backgroundColour,
                                },
                                new CircularContainer
                                {
                                    Masking = true,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(progress_end_radius),
                                    Children = new Drawable[]
                                    {
                                        bgProgress = new CircularProgress
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Rotation = 180,
                                            Colour = backgroundColour,
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Name = @"Progress under covers for smoothing",
                                            RelativeSizeAxes = Axes.Both,
                                            Rotation = 180,
                                            Child = volumeCircle = new CircularProgress
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                            }
                                        },
                                    }
                                },
                                new Circle
                                {
                                    Name = @"Inner Cover",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = backgroundColour,
                                    Size = new Vector2(progress_start_radius),
                                },
                                new Container
                                {
                                    Name = @"Progress overlay for glow",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(progress_start_radius + progress_size / 1.5f),
                                    Rotation = 180,
                                    Padding = new MarginPadding(-Blur.KernelSize(blur_amount)),
                                    Child = (volumeCircleGlow = new CircularProgress
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        InnerRadius = progress_size * 0.8f,
                                    }).WithEffect(new GlowEffect
                                    {
                                        Colour = meterColour,
                                        BlurSigma = new Vector2(blur_amount),
                                        Strength = 5,
                                        PadExtent = true
                                    }),
                                },
                            },
                        },
                        selectedGlowContainer = new CircularContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Glow,
                                Colour = meterColour.Opacity(0.1f),
                                Radius = 10,
                            }
                        },
                        maxGlow = (text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Numeric.With(size: 0.16f * CircleSize)
                        }).WithEffect(new GlowEffect
                        {
                            Colour = Color4.Transparent,
                            PadExtent = true,
                        })
                    }
                },
                new Container
                {
                    Size = LABEL_SIZE,
                    CornerRadius = 10,
                    Masking = true,
                    Margin = new MarginPadding { Left = CircleSize + 10 },
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            Text = name
                        }
                    }
                }
            };

            Bindable.BindValueChanged(volume =>
            {
                decibel = DecibelFromLinear(volume.NewValue);
                this.TransformTo(nameof(DisplayVolume), decibel, 400, Easing.OutQuint);
            }, true);

            bgProgress.Progress = 0.75f;
        }

        private int currentStep;
        private const int step_min = (int)(DB_MIN / DB_PRECISION);
        private const int step_max = (int)(DB_MAX / DB_PRECISION);

        private double decibel;
        private double displayVolume;
        private double normalizedVolume;

        protected double DisplayVolume
        {
            get => displayVolume;
            set
            {
                normalizedVolume = (value - DB_MIN) / (DB_MAX - DB_MIN);

                int step = (int)Math.Round(value / DB_PRECISION);
                bool stepChanged = step != currentStep;

                currentStep = step;
                displayVolume = currentStep * DB_PRECISION;

                if (currentStep >= step_max)
                {
                    text.Text = "MAX";
                    maxGlow.EffectColour = meterColour.Opacity(2f);
                }
                else
                {
                    maxGlow.EffectColour = Color4.Transparent;
                    text.Text = currentStep <= step_min ? "-INF" : displayVolume.ToString("N1", CultureInfo.CurrentCulture);
                }

                volumeCircle.Progress = normalizedVolume * 0.75f;
                volumeCircleGlow.Progress = normalizedVolume * 0.75f;

                if (stepChanged && IsLoaded)
                    Scheduler.AddOnce(playTickSound);
            }
        }

        private void playTickSound()
        {
            const int tick_debounce_time = 30;

            if (Time.Current - sampleLastPlaybackTime <= tick_debounce_time)
                return;

            var channel = notchSample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + (normalizedVolume * 0.1f);

            // intentionally pitched down, even when hitting max.
            if (currentStep == step_min || currentStep == step_max)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
            sampleLastPlaybackTime = Time.Current;
        }

        public double Volume
        {
            get => Bindable.Value;
            private set => Bindable.Value = value;
        }

        public void Increase(double amount = 1, bool isPrecise = false) => adjust(amount, isPrecise);
        public void Decrease(double amount = 1, bool isPrecise = false) => adjust(-amount, isPrecise);

        // because volume precision is set to 0.01, this local is required to keep track of more precise adjustments and only apply when possible.
        private double scrollAccumulation;

        private double accelerationModifier = 1;

        private const double max_acceleration = 5;
        private const double acceleration_multiplier = 1.8;

        private ScheduledDelegate accelerationDebounce;

        private void resetAcceleration() => accelerationModifier = 1;

        private float dragDelta;

        protected override bool OnDragStart(DragStartEvent e)
        {
            dragDelta = 0;
            adjustFromDrag(e.Delta);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            adjustFromDrag(e.Delta);
            base.OnDrag(e);
        }

        private void adjustFromDrag(Vector2 delta)
        {
            const float mouse_drag_divisor = (float)(2 / DB_PRECISION);

            dragDelta += delta.Y / mouse_drag_divisor;

            if (Math.Abs(dragDelta) < DB_PRECISION) return;

            Volume = LinearFromDecibel(decibel - dragDelta);
            dragDelta = 0;
        }

        private void adjust(double delta, bool isPrecise)
        {
            if (delta == 0)
                return;

            // every adjust increment increases the rate at which adjustments happen up to a cutoff.
            // this debounce will reset on inactivity.
            accelerationDebounce?.Cancel();
            accelerationDebounce = Scheduler.AddDelayed(resetAcceleration, 150);

            delta *= accelerationModifier;
            accelerationModifier = Math.Min(max_acceleration, accelerationModifier * acceleration_multiplier);

            double dB = decibel;
            const double precision = DB_PRECISION;

            if (isPrecise)
            {
                scrollAccumulation += delta * precision;

                while (Precision.AlmostBigger(Math.Abs(scrollAccumulation), precision))
                {
                    dB += Math.Sign(scrollAccumulation) * precision;
                    scrollAccumulation = scrollAccumulation < 0 ? Math.Min(0, scrollAccumulation + precision) : Math.Max(0, scrollAccumulation - precision);
                }
            }
            else
            {
                dB += Math.Sign(delta) * Math.Max(precision, Math.Abs(delta * precision));
            }

            Volume = LinearFromDecibel(dB);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            adjust(e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            State = SelectionState.Selected;
            return base.OnMouseMove(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            State = SelectionState.Selected;
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void updateSelectedState()
        {
            switch (state)
            {
                case SelectionState.Selected:
                    this.ScaleTo(1.04f, transition_length, Easing.OutExpo);
                    selectedGlowContainer.FadeIn(transition_length, Easing.OutExpo);
                    hoverSample?.Play();
                    break;

                case SelectionState.NotSelected:
                    this.ScaleTo(1f, transition_length, Easing.OutExpo);
                    selectedGlowContainer.FadeOut(transition_length, Easing.OutExpo);
                    break;
            }
        }
    }
}
