// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupTickSliderBar : OsuSliderBar<float>
    {
        private const float default_height = 20;
        private const float default_slider_height = 8;
        private const float nub_size_x = 36;
        private const float nub_size_y = 20;
        private const float default_caption_text_size = 13;

        private readonly Box leftBox;
        private readonly Box rightBox;
        private readonly OsuSpriteText leftTickCaption;
        private readonly OsuSpriteText middleTickCaption;
        private readonly OsuSpriteText rightTickCaption;
        private readonly Ticks ticks;

        protected override bool ChangeNubValue => false;

        public string TooltipTextSuffix = "";
        public override string TooltipText => $"{base.TooltipText}{TooltipTextSuffix}";

        private float normalPrecision;
        public float NormalPrecision
        {
            get => normalPrecision;
            set
            {
                if (normalPrecision == value)
                    return;

                normalPrecision = value;
                if (ticks != null)
                    ticks.ValueInterval = value;
                if (!isUsingAlternatePrecision)
                    Precision = value;
            }
        }
        private float alternatePrecision;
        public float AlternatePrecision
        {
            get => alternatePrecision;
            set
            {
                alternatePrecision = value;
                if (isUsingAlternatePrecision)
                    Precision = value;
            }
        }
        private bool isUsingAlternatePrecision;
        public bool IsUsingAlternatePrecision
        {
            get => isUsingAlternatePrecision;
            set
            {
                if (value == isUsingAlternatePrecision)
                    return;

                isUsingAlternatePrecision = value;
                Precision = value ? AlternatePrecision : NormalPrecision;
            }
        }

        public float MinValue
        {
            get => CurrentNumber.MinValue;
            set
            {
                if (CurrentNumber.MinValue == value)
                    return;

                CurrentNumber.MinValue = value;
                if (ticks != null)
                    ticks.MinValue = value;
                if (CurrentNumber.Value < value)
                    CurrentNumber.Value = value;
            }
        }
        public float MaxValue
        {
            get => CurrentNumber.MaxValue;
            set
            {
                if (CurrentNumber.MaxValue == value)
                    return;

                CurrentNumber.MaxValue = value;
                if (ticks != null)
                    ticks.MaxValue = value;
                if (CurrentNumber.Value > value)
                    CurrentNumber.Value = value;
            }
        }
        public float Precision
        {
            get => CurrentNumber.Precision;
            private set => CurrentNumber.Precision = value;
        }

        public string LeftTickCaption
        {
            get => leftTickCaption.Text;
            set => leftTickCaption.Text = value;
        }
        public string MiddleTickCaption
        {
            get => middleTickCaption.Text;
            set => middleTickCaption.Text = value;
        }
        public string RightTickCaption
        {
            get => rightTickCaption.Text;
            set => rightTickCaption.Text = value;
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                leftBox.Colour = value;
            }
        }

        public SetupTickSliderBar(float minValue, float maxValue, float normalPrecision, float alternatePrecision)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Precision = normalPrecision;
            NormalPrecision = normalPrecision;
            AlternatePrecision = alternatePrecision;

            RelativeSizeAxes = Axes.X;
            Height = default_height;
            RangePadding = 20;
            Y = 5;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = default_height,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Height = default_height,
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Height = default_slider_height,
                                    RelativeSizeAxes = Axes.X,
                                    CornerRadius = 3,
                                    Masking = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Children = new Drawable[]
                                    {
                                        leftBox = new Box
                                        {
                                            Height = default_slider_height,
                                            RelativeSizeAxes = Axes.X,
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                        },
                                        rightBox = new Box
                                        {
                                            Height = default_slider_height,
                                            RelativeSizeAxes = Axes.X,
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            Colour = Color4.Black
                                        },
                                    }
                                },
                                Nub = new Nub
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopCentre,
                                    RelativePositionAxes = Axes.X,
                                    Size = new Vector2(nub_size_x, nub_size_y)
                                },
                            }
                        },
                        ticks = new Ticks(minValue, maxValue, normalPrecision),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Children = new[]
                            {
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Child = leftTickCaption = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Alpha = 0.8f,
                                        TextSize = default_caption_text_size,
                                    }
                                },
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Child = middleTickCaption = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Alpha = 0.8f,
                                        TextSize = default_caption_text_size,
                                    }
                                },
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Child = rightTickCaption = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Alpha = 0.8f,
                                        TextSize = default_caption_text_size,
                                    }
                                }
                            },
                        }
                    }
                },
                new HoverClickSounds()
            };

            Nub.Current.Value = true;

            ticks.TickClicked += a => Current.Value = a;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.BlueDark;
            Nub.Colour = colours.BlueDark;
            Nub.AccentColour = colours.BlueDark;
            Nub.GlowColour = colours.BlueDarker;
            Nub.GlowingAccentColour = colours.BlueLighter;
            leftTickCaption.Colour = colours.BlueDark;
            middleTickCaption.Colour = colours.BlueDark;
            rightTickCaption.Colour = colours.BlueDark;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            IsUsingAlternatePrecision = state.Keyboard.ShiftPressed;
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            IsUsingAlternatePrecision = state.Keyboard.ShiftPressed;
            return base.OnKeyUp(state, args);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(value, 250, Easing.OutQuint);
            leftBox.ResizeWidthTo(value, 250, Easing.OutQuint);
            rightBox.ResizeWidthTo(1 - value, 250, Easing.OutQuint);
        }

        private class Ticks : Container
        {
            private readonly bool canCreateTicks;

            public event Action<float> TickClicked;

            private float min;
            public float MinValue
            {
                get => min;
                set
                {
                    min = value;
                    if (canCreateTicks)
                        createTicks();
                }
            }

            private float max;
            public float MaxValue
            {
                get => max;
                set
                {
                    max = value;
                    if (canCreateTicks)
                        createTicks();
                }
            }

            private float interval;
            public float ValueInterval
            {
                get => interval;
                set
                {
                    interval = value;
                    if (canCreateTicks)
                        createTicks();
                }
            }

            public Ticks(float minValue, float maxValue, float valueInterval)
            {
                MinValue = minValue;
                MaxValue = maxValue;
                ValueInterval = valueInterval;

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;

                canCreateTicks = true;
                createTicks();
            }

            private void createTicks()
            {
                ClearInternal();
                if (MinValue == MaxValue)
                    return;

                for (float i = MinValue; i <= MaxValue; i += ValueInterval)
                    AddInternal(new Tick(i) { X = getPosition(i), RelativePositionAxes = Axes.X });
                foreach (var c in InternalChildren)
                    if (c is Tick t)
                        t.TickClicked += OnTickClicked;
            }

            protected void OnTickClicked(float value)
            {
                TickClicked?.Invoke(value);
            }

            private float getPosition(float value) => (value - min) / (max - min);
        }

        private class Tick : ClickableContainer
        {
            private readonly float value;
            private readonly Box box;

            public event Action<float> TickClicked;

            public Tick(float value)
            {
                this.value = value;
                Origin = Anchor.TopCentre;
                Size = new Vector2(7, 8);

                InternalChild = box = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(1, 2),
                    Alpha = 0.8f
                };

                CornerRadius = 0.25f;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                box.Colour = colours.BlueDark;
            }

            protected override bool OnClick(InputState state)
            {
                TickClicked?.Invoke(value);
                return base.OnClick(state);
            }
        }
    }
}
