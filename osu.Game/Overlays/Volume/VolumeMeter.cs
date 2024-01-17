// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Globalization;
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

namespace osu.Game.Overlays.Volume
{
    public partial class VolumeMeter : Container, IStateful<SelectionState>
    {
        private CircularProgress volumeCircle;
        private CircularProgress volumeCircleGlow;

        public BindableDouble Bindable { get; } = new BindableDouble { MinValue = 0, MaxValue = 1, Precision = 0.01 };
        private readonly float circleSize;
        private readonly Color4 meterColour;
        private readonly string name;

        private OsuSpriteText text;
        private BufferedContainer maxGlow;

        private Container selectedGlowContainer;

        private Sample hoverSample;
        private Sample notchSample;
        private double sampleLastPlaybackTime;

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
            this.circleSize = circleSize;
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
                    Size = new Vector2(circleSize),
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
                            Font = OsuFont.Numeric.With(size: 0.16f * circleSize)
                        }).WithEffect(new GlowEffect
                        {
                            Colour = Color4.Transparent,
                            PadExtent = true,
                        })
                    }
                },
                new Container
                {
                    Size = new Vector2(120, 20),
                    CornerRadius = 10,
                    Masking = true,
                    Margin = new MarginPadding { Left = circleSize + 10 },
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

            Bindable.BindValueChanged(volume => { this.TransformTo(nameof(DisplayVolume), volume.NewValue, 400, Easing.OutQuint); }, true);

            bgProgress.Current.Value = 0.75f;
        }

        private int? displayVolumeInt;

        private double displayVolume;

        protected double DisplayVolume
        {
            get => displayVolume;
            set
            {
                displayVolume = value;

                int intValue = (int)Math.Round(displayVolume * 100);
                bool intVolumeChanged = intValue != displayVolumeInt;

                displayVolumeInt = intValue;

                if (displayVolume >= 0.995f)
                {
                    text.Text = "MAX";
                    maxGlow.EffectColour = meterColour.Opacity(2f);
                }
                else
                {
                    maxGlow.EffectColour = Color4.Transparent;
                    text.Text = intValue.ToString(CultureInfo.CurrentCulture);
                }

                volumeCircle.Current.Value = displayVolume * 0.75f;
                volumeCircleGlow.Current.Value = displayVolume * 0.75f;

                if (intVolumeChanged && IsLoaded)
                    Scheduler.AddOnce(playTickSound);
            }
        }

        private void playTickSound()
        {
            const int tick_debounce_time = 30;

            if (Time.Current - sampleLastPlaybackTime <= tick_debounce_time)
                return;

            var channel = notchSample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + displayVolume * 0.1f;

            // intentionally pitched down, even when hitting max.
            if (displayVolumeInt == 0 || displayVolumeInt == 100)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
            sampleLastPlaybackTime = Time.Current;
        }

        public double Volume
        {
            get => Bindable.Value;
            private set => Bindable.Value = value;
        }

        private const double adjust_step = 0.01;

        public void Increase(double amount = 1, bool isPrecise = false) => adjust(amount, isPrecise);
        public void Decrease(double amount = 1, bool isPrecise = false) => adjust(-amount, isPrecise);

        // because volume precision is set to 0.01, this local is required to keep track of more precise adjustments and only apply when possible.
        private double scrollAccumulation;

        private double accelerationModifier = 1;

        private const double max_acceleration = 5;
        private const double acceleration_multiplier = 1.8;

        private ScheduledDelegate accelerationDebounce;

        private void resetAcceleration() => accelerationModifier = 1;

        private float dragDelta = 0;

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
            const float mouse_drag_divisor = 200;

            dragDelta += delta.Y / mouse_drag_divisor;

            if (Math.Abs(dragDelta) < 0.01) return;

            Volume -= dragDelta;
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

            double precision = Bindable.Precision;

            if (isPrecise)
            {
                scrollAccumulation += delta * adjust_step;

                while (Precision.AlmostBigger(Math.Abs(scrollAccumulation), precision))
                {
                    Volume += Math.Sign(scrollAccumulation) * precision;
                    scrollAccumulation = scrollAccumulation < 0 ? Math.Min(0, scrollAccumulation + precision) : Math.Max(0, scrollAccumulation - precision);
                }
            }
            else
            {
                Volume += Math.Sign(delta) * Math.Max(precision, Math.Abs(delta * adjust_step));
            }
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
