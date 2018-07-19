// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System;
using System.Globalization;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuSetupTickSliderBar : SliderBar<float>, IHasTooltip, IHasAccentColour
    {
        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        public const int MAX_DECIMAL_DIGITS = 5;
        public const float DEFAULT_HEIGHT = 20;
        public const float DEFAULT_SLIDER_HEIGHT = 8;
        public const float NUB_SIZE_X = 36;
        public const float NUB_SIZE_Y = 20;
        public const float DEFAULT_CAPTION_TEXT_SIZE = 13;

        private bool leftShiftHeld;
        private bool rightShiftHeld;

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

        private SampleChannel sample;
        private double lastSampleTime;
        private float lastSampleValue;

        protected readonly Nub Nub;
        private readonly Box leftBox;
        private readonly Box rightBox;
        private readonly OsuSpriteText leftTickCaption;
        private readonly OsuSpriteText middleTickCaption;
        private readonly OsuSpriteText rightTickCaption;
        private readonly Ticks ticks;

        public string TooltipTextSuffix = "";

        public virtual string TooltipText
        {
            get
            {
                var floatValue = CurrentNumber.Value;
                var floatPrecision = CurrentNumber.Precision;

                var decimalPrecision = normalise((decimal)floatPrecision, MAX_DECIMAL_DIGITS);

                // Find the number of significant digits (we could have less than 5 after normalize())
                var significantDigits = findPrecision(decimalPrecision);

                return $"{floatValue.ToString($"N{significantDigits}")}{TooltipTextSuffix}";
            }
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

        public OsuSetupTickSliderBar(float minValue, float maxValue, float normalPrecision, float alternatePrecision)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Precision = normalPrecision;
            NormalPrecision = normalPrecision;
            AlternatePrecision = alternatePrecision;

            RelativeSizeAxes = Axes.X;
            Height = DEFAULT_HEIGHT;
            RangePadding = 20;
            Y = 5;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_HEIGHT,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Height = DEFAULT_HEIGHT,
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Height = DEFAULT_SLIDER_HEIGHT,
                                    RelativeSizeAxes = Axes.X,
                                    CornerRadius = 3,
                                    Masking = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Children = new Drawable[]
                                    {
                                        leftBox = new Box
                                        {
                                            Height = DEFAULT_SLIDER_HEIGHT,
                                            RelativeSizeAxes = Axes.X,
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                        },
                                        rightBox = new Box
                                        {
                                            Height = DEFAULT_SLIDER_HEIGHT,
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
                                    Size = new Vector2(NUB_SIZE_X, NUB_SIZE_Y)
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
                                        TextSize = DEFAULT_CAPTION_TEXT_SIZE,
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
                                        TextSize = DEFAULT_CAPTION_TEXT_SIZE,
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
                                        TextSize = DEFAULT_CAPTION_TEXT_SIZE,
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
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Sample.Get(@"UI/sliderbar-notch");
            AccentColour = colours.BlueDark;
            Nub.Colour = colours.BlueDark;
            Nub.AccentColour = colours.BlueDark;
            Nub.GlowColour = colours.BlueDarker;
            Nub.GlowingAccentColour = colours.BlueLighter;
            leftTickCaption.Colour = colours.BlueDark;
            middleTickCaption.Colour = colours.BlueDark;
            rightTickCaption.Colour = colours.BlueDark;
        }

        protected override bool OnHover(InputState state)
        {
            Nub.Glowing = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            Nub.Glowing = false;
            base.OnHoverLost(state);
        }
        
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            leftShiftHeld |= args.Key == Key.ShiftLeft;
            rightShiftHeld |= args.Key == Key.ShiftRight;
            IsUsingAlternatePrecision = leftShiftHeld | rightShiftHeld;

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            leftShiftHeld &= args.Key != Key.ShiftLeft;
            rightShiftHeld &= args.Key != Key.ShiftRight;
            IsUsingAlternatePrecision = leftShiftHeld | rightShiftHeld;

            return base.OnKeyUp(state, args);
        }

        protected override void OnUserChange()
        {
            base.OnUserChange();
            playSample();
        }

        private void playSample()
        {
            if (Clock == null || Clock.CurrentTime - lastSampleTime <= 50)
                return;

            if (Current.Value.Equals(lastSampleValue))
                return;

            lastSampleValue = Current.Value;

            lastSampleTime = Clock.CurrentTime;
            sample.Frequency.Value = 1 + NormalizedValue * 0.2f;

            if (NormalizedValue == 0)
                sample.Frequency.Value -= 0.4f;
            else if (NormalizedValue == 1)
                sample.Frequency.Value += 0.4f;

            sample.Play();
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(value, 250, Easing.OutQuint);
            leftBox.ResizeWidthTo(value, 250, Easing.OutQuint);
            rightBox.ResizeWidthTo(1 - value, 250, Easing.OutQuint);
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private decimal normalise(decimal d, int sd) => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        /// <summary>
        /// Finds the number of digits after the decimal.
        /// </summary>
        /// <param name="d">The value to find the number of decimal digits for.</param>
        /// <returns>The number decimal digits.</returns>
        private int findPrecision(decimal d)
        {
            int precision = 0;
            while (d != Math.Round(d))
            {
                d *= 10;
                precision++;
            }

            return precision;
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
