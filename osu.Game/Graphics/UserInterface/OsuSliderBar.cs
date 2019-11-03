// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuSliderBar<T> : SliderBar<T>, IHasTooltip, IHasAccentColour
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
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

        public virtual string TooltipText { get; private set; }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
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

            Current.DisabledChanged += disabled => { Alpha = disabled ? 0.3f : 1; };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sample = audio.Samples.Get(@"UI/sliderbar-notch");
            AccentColour = colours.Pink;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentNumber.BindValueChanged(current => updateTooltipText(current.NewValue), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Nub.Glowing = true;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Nub.Glowing = false;
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Nub.Current.Value = true;
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            Nub.Current.Value = false;
            return base.OnMouseUp(e);
        }

        protected override void OnUserChange(T value)
        {
            base.OnUserChange(value);
            playSample(value);
            updateTooltipText(value);
        }

        private void playSample(T value)
        {
            if (Clock == null || Clock.CurrentTime - lastSampleTime <= 50)
                return;

            if (value.Equals(lastSampleValue))
                return;

            lastSampleValue = value;

            lastSampleTime = Clock.CurrentTime;
            sample.Frequency.Value = 1 + NormalizedValue * 0.2f;

            if (NormalizedValue == 0)
                sample.Frequency.Value -= 0.4f;
            else if (NormalizedValue == 1)
                sample.Frequency.Value += 0.4f;

            sample.Play();
        }

        private void updateTooltipText(T value)
        {
            if (CurrentNumber.IsInteger)
                TooltipText = ((int)Convert.ChangeType(value, typeof(int))).ToString("N0");
            else
            {
                double floatValue = (double)Convert.ChangeType(value, typeof(double));
                double floatMinValue = (double)Convert.ChangeType(CurrentNumber.MinValue, typeof(double));
                double floatMaxValue = (double)Convert.ChangeType(CurrentNumber.MaxValue, typeof(double));

                if (floatMaxValue == 1 && floatMinValue >= -1)
                    TooltipText = floatValue.ToString("P0");
                else
                {
                    var decimalPrecision = normalise((decimal)Convert.ChangeType(CurrentNumber.Precision, typeof(decimal)), max_decimal_digits);

                    // Find the number of significant digits (we could have less than 5 after normalize())
                    var significantDigits = findPrecision(decimalPrecision);

                    TooltipText = floatValue.ToString($"N{significantDigits}");
                }
            }
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
