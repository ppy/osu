// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Globalization;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuSliderBar<T> : SliderBar<T>, IHasTooltip, IHasAccentColour
        where T : struct, IEquatable<T>, IComparable, IConvertible
    {
        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        private const int max_decimal_digits = 5;

        private SampleChannel sample;
        private double lastSampleTime;
        private T lastSampleValue;

        protected readonly Nub Nub;
        private readonly Box leftBox;
        private readonly Box rightBox;

        public virtual string TooltipText
        {
            get
            {
                var bindableDouble = CurrentNumber as BindableNumber<double>;
                var bindableFloat = CurrentNumber as BindableNumber<float>;
                var floatValue = bindableDouble?.Value ?? bindableFloat?.Value;
                var floatPrecision = bindableDouble?.Precision ?? bindableFloat?.Precision;

                if (floatValue != null)
                {
                    var floatMinValue = bindableDouble?.MinValue ?? bindableFloat.MinValue;
                    var floatMaxValue = bindableDouble?.MaxValue ?? bindableFloat.MaxValue;

                    if (floatMaxValue == 1 && (floatMinValue == 0 || floatMinValue == -1))
                        return floatValue.Value.ToString("P0");

                    var decimalPrecision = normalise((decimal)floatPrecision, max_decimal_digits);

                    // Find the number of significant digits (we could have less than 5 after normalize())
                    var significantDigits = findPrecision(decimalPrecision);

                    return floatValue.Value.ToString($"N{significantDigits}");
                }

                var bindableInt = CurrentNumber as BindableNumber<int>;
                if (bindableInt != null)
                    return bindableInt.Value.ToString("N0");

                return Current.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
                leftBox.Colour = value;
                rightBox.Colour = value;
            }
        }

        public OsuSliderBar()
        {
            Height = 12;
            RangePadding = 20;
            Children = new Drawable[]
            {
                leftBox = new Box
                {
                    Height = 2,
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    Position = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.None,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                rightBox = new Box
                {
                    Height = 2,
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    Position = new Vector2(-2, 0),
                    RelativeSizeAxes = Axes.None,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Alpha = 0.5f,
                },
                Nub = new Nub
                {
                    Origin = Anchor.TopCentre,
                    Expanded = true,
                },
                new HoverClickSounds()
            };

            Current.DisabledChanged += disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Sample.Get(@"UI/sliderbar-notch");
            AccentColour = colours.Pink;
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
            Nub.Current.Value = true;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Nub.Current.Value = false;
            return base.OnMouseUp(state, args);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            leftBox.Scale = new Vector2(MathHelper.Clamp(
                Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, DrawWidth), 1);
            rightBox.Scale = new Vector2(MathHelper.Clamp(
                DrawWidth - Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, DrawWidth), 1);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(RangePadding + UsableWidth * value, 250, Easing.OutQuint);
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private decimal normalise(decimal d, int sd)
            => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

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
    }
}
