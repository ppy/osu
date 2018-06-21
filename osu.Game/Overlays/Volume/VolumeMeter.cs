// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public class VolumeMeter : Container, IKeyBindingHandler<GlobalAction>
    {
        private CircularProgress volumeCircle;
        private CircularProgress volumeCircleGlow;

        public BindableDouble Bindable { get; } = new BindableDouble { MinValue = 0, MaxValue = 1 };
        private readonly float circleSize;
        private readonly Color4 meterColour;
        private readonly string name;

        private OsuSpriteText text;
        private BufferedContainer maxGlow;

        public VolumeMeter(string name, float circleSize, Color4 meterColour)
        {
            this.circleSize = circleSize;
            this.meterColour = meterColour;
            this.name = name;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
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
                            Alpha = 0.9f,
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
                                            Name = "Progress under covers for smoothing",
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
                                    Name = "Inner Cover",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = backgroundColour,
                                    Size = new Vector2(progress_start_radius),
                                },
                                new Container
                                {
                                    Name = "Progress overlay for glow",
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
                        maxGlow = (text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = "Venera",
                            TextSize = 0.16f * circleSize
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
                            Alpha = 0.9f,
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = "Exo2.0-Bold",
                            Text = name
                        }
                    }
                }
            };
            Bindable.ValueChanged += newVolume =>
            {
                this.TransformTo("DisplayVolume",
                    newVolume,
                    400,
                    Easing.OutQuint);
            };
            bgProgress.Current.Value = 0.75f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Bindable.TriggerChange();
        }

        private double displayVolume;

        protected double DisplayVolume
        {
            get => displayVolume;
            set
            {
                displayVolume = value;

                if (displayVolume > 0.99f)
                {
                    text.Text = "MAX";
                    maxGlow.EffectColour = meterColour.Opacity(2f);
                }
                else
                {
                    maxGlow.EffectColour = Color4.Transparent;
                    text.Text = Math.Round(displayVolume * 100).ToString(CultureInfo.CurrentCulture);
                }

                volumeCircle.Current.Value = displayVolume * 0.75f;
                volumeCircleGlow.Current.Value = displayVolume * 0.75f;
            }
        }

        public double Volume
        {
            get => Bindable;
            private set => Bindable.Value = value;
        }

        private const float adjust_step = 0.05f;

        public void Increase() => adjust(1);
        public void Decrease() => adjust(-1);

        private void adjust(int direction)
        {
            float amount = adjust_step * direction;

            // handle the case where the OnPressed action was actually a mouse wheel.
            // this allows for precise wheel handling.
            var state = GetContainingInputManager().CurrentState;
            if (state.Mouse?.ScrollDelta.Y != 0)
            {
                OnScroll(state);
                return;
            }

            Volume += amount;
        }

        public bool OnPressed(GlobalAction action)
        {
            if (!IsHovered) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    Decrease();
                    return true;
                case GlobalAction.IncreaseVolume:
                    Increase();
                    return true;
            }

            return false;
        }

        // because volume precision is set to 0.01, this local is required to keep track of more precise adjustments and only apply when possible.
        private double scrollAmount;

        protected override bool OnScroll(InputState state)
        {
            scrollAmount += adjust_step * state.Mouse.ScrollDelta.Y * (state.Mouse.HasPreciseScroll ? 0.1f : 1);

            if (Math.Abs(scrollAmount) < Bindable.Precision)
                return true;

            Volume += scrollAmount;
            scrollAmount = 0;
            return true;
        }

        public bool OnReleased(GlobalAction action) => false;

        private const float transition_length = 500;

        protected override bool OnHover(InputState state)
        {
            this.ScaleTo(1.04f, transition_length, Easing.OutExpo);
            return false;
        }

        protected override void OnHoverLost(InputState state)
        {
            this.ScaleTo(1f, transition_length, Easing.OutExpo);
        }
    }
}
