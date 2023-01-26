// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Utils;

namespace osu.Game.Graphics.UserInterface
{
    public abstract partial class OsuSliderBar<T> : SliderBar<T>, IHasTooltip
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public virtual LocalisableString TooltipText { get; private set; }

        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        private const int max_decimal_digits = 5;

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentNumber.BindValueChanged(current => TooltipText = getTooltipText(current.NewValue), true);
        }

        protected override void OnUserChange(T value)
        {
            base.OnUserChange(value);
            TooltipText = getTooltipText(value);
        }

        private LocalisableString getTooltipText(T value)
        {
            if (CurrentNumber.IsInteger)
                return value.ToInt32(NumberFormatInfo.InvariantInfo).ToString("N0");

            double floatValue = value.ToDouble(NumberFormatInfo.InvariantInfo);

            if (DisplayAsPercentage)
                return floatValue.ToString("0%");

            decimal decimalPrecision = normalise(CurrentNumber.Precision.ToDecimal(NumberFormatInfo.InvariantInfo), max_decimal_digits);

            // Find the number of significant digits (we could have less than 5 after normalize())
            int significantDigits = FormatUtils.FindPrecision(decimalPrecision);

            return floatValue.ToString($"N{significantDigits}");
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private decimal normalise(decimal d, int sd)
            => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
