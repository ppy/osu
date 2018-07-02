// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class OsuTickSliderBar : SliderBar<float>, IHasTooltip, IHasAccentColour
    {
        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        private const int MAX_DECIMAL_DIGITS = 5;
        private const float DEFAULT_HEIGHT = 20;
        private const float DEFAULT_SLIDER_HEIGHT = 8;
        private const float NUB_SIZE_X = 36;
        private const float NUB_SIZE_Y = 20;

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
        private readonly Container sliderContainer;
        private readonly Box leftBox;
        private readonly Box rightBox;
        private readonly OsuSpriteText leftTickCaption;
        private readonly OsuSpriteText middleTickCaption;
        private readonly OsuSpriteText rightTickCaption;
        private readonly Ticks ticks;

        public virtual string TooltipText
        {
            get
            {
                var bindableFloat = CurrentNumber as BindableNumber<float>;
                var floatValue = bindableFloat?.Value;
                var floatPrecision = bindableFloat?.Precision;

                var decimalPrecision = normalise((decimal)floatPrecision, MAX_DECIMAL_DIGITS);

                // Find the number of significant digits (we could have less than 5 after normalize())
                var significantDigits = findPrecision(decimalPrecision);

                return floatValue.Value.ToString($"N{significantDigits}");
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

        public OsuTickSliderBar(float minValue, float maxValue, float normalPrecision, float alternatePrecision)
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
                    //Anchor = Anchor.CentreLeft,
                    //Origin = Anchor.CentreLeft,
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
                                sliderContainer = new Container
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
                                    Origin = Anchor.TopLeft,
                                    //RelativePositionAxes = Axes.X,
                                    Size = new Vector2(NUB_SIZE_X, NUB_SIZE_Y)
                                },
                            }
                        },
                        ticks = new Ticks(minValue, maxValue, normalPrecision),
                        new Container
                        {
                            Children = new[]
                            {
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Child = leftTickCaption = new OsuSpriteText()
                                },
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    X = 0.5f,
                                    Child = middleTickCaption = new OsuSpriteText()
                                },
                                new Container
                                {
                                    RelativePositionAxes = Axes.X,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    X = 1,
                                    Child = rightTickCaption = new OsuSpriteText()
                                }
                            },
                        }
                    }
                },
                new HoverClickSounds()
            };

            Nub.Current.Value = true;

            ticks.TickClicked += a => Current.Value = a;

            // Might remove since this control is not supposed to be disabled probably.
            Current.DisabledChanged += d => Alpha = d ? 0.5f : 1;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Sample.Get(@"UI/sliderbar-notch");
            AccentColour = colours.BlueLight;
            Nub.Colour = colours.BlueLight;
            Nub.AccentColour = colours.BlueLight;
            Nub.GlowColour = colours.BlueDarker;
            Nub.GlowingAccentColour = colours.BlueLighter;
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

        protected override void OnSizingChanged()
        {
            if (Nub != null)
                Nub.MoveToX((UsableWidth + NUB_SIZE_X / 2) * Current.Value - NUB_SIZE_X / 2, 0);
            base.OnSizingChanged();
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

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            return base.OnMouseUp(state, args);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            leftBox.Scale = new Vector2(MathHelper.Clamp(Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, DrawWidth), 1);
            rightBox.Scale = new Vector2(MathHelper.Clamp(DrawWidth - Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, DrawWidth), 1);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX((UsableWidth + NUB_SIZE_X / 2) * value - NUB_SIZE_X / 2, 250, Easing.OutQuint);
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
            private bool canUpdateTicks;

            public event Action<float> TickClicked;

            private float min;
            public float MinValue
            {
                get => min;
                set
                {
                    min = value;
                    if (canUpdateTicks)
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
                    if (canUpdateTicks)
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
                    if (canUpdateTicks)
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

                canUpdateTicks = true;
                createTicks();
            }

            private void createTicks()
            {
                ClearInternal();
                if (MinValue == MaxValue)
                    return;

                for (float i = MinValue; i <= MaxValue; i += ValueInterval)
                    AddInternal(new Tick(i) { X = getPosition(i), RelativePositionAxes = Axes.X });
                foreach (Tick t in InternalChildren)
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
                Size = new Vector2(5, 6);

                InternalChild = box = new Box
                {
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(1, 2)
                };

                CornerRadius = 0.25f;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                box.Colour = colours.BlueLight;
            }

            protected override bool OnClick(InputState state)
            {
                TickClicked?.Invoke(value);
                return base.OnClick(state);
            }
        }
    }
}
