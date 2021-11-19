// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    /// <summary>
    /// Analogous to <see cref="SliderWithTextBoxInput{T}"/>, but supports scenarios
    /// where multiple objects with multiple different property values are selected
    /// by providing an "indeterminate state".
    /// </summary>
    public class IndeterminateSliderWithTextBoxInput<T> : CompositeDrawable, IHasCurrentValue<T?>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => slider.KeyboardStep;
            set => slider.KeyboardStep = value;
        }

        private readonly BindableWithCurrent<T?> current = new BindableWithCurrent<T?>();

        public Bindable<T?> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly SettingsSlider<T> slider;
        private readonly LabelledTextBox textbox;

        /// <summary>
        /// Creates an <see cref="IndeterminateSliderWithTextBoxInput{T}"/>.
        /// </summary>
        /// <param name="labelText">The label text for the slider and text box.</param>
        /// <param name="indeterminateValue">
        /// Bindable to use for the slider until a non-null value is set for <see cref="Current"/>.
        /// In particular, it can be used to control min/max bounds and precision in the case of <see cref="BindableNumber{T}"/>s.
        /// </param>
        public IndeterminateSliderWithTextBoxInput(LocalisableString labelText, Bindable<T> indeterminateValue)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        textbox = new LabelledTextBox
                        {
                            Label = labelText,
                        },
                        slider = new SettingsSlider<T>
                        {
                            TransferValueOnCommit = true,
                            RelativeSizeAxes = Axes.X,
                            Current = indeterminateValue
                        }
                    }
                },
            };

            textbox.OnCommit += (t, isNew) =>
            {
                if (!isNew) return;

                try
                {
                    slider.Current.Parse(t.Text);
                }
                catch
                {
                    // TriggerChange below will restore the previous text value on failure.
                }

                // This is run regardless of parsing success as the parsed number may not actually trigger a change
                // due to bindable clamping. Even in such a case we want to update the textbox to a sane visual state.
                Current.TriggerChange();
            };
            slider.Current.BindValueChanged(val => Current.Value = val.NewValue);

            Current.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            if (Current.Value is T nonNullValue)
            {
                slider.Current.Value = nonNullValue;

                // use the value from the slider to ensure that any precision/min/max set on it via the initial indeterminate value have been applied correctly.
                decimal decimalValue = slider.Current.Value.ToDecimal(NumberFormatInfo.InvariantInfo);
                textbox.Text = decimalValue.ToString($@"N{FormatUtils.FindPrecision(decimalValue)}");
                textbox.PlaceholderText = string.Empty;
            }
            else
            {
                textbox.Text = null;
                textbox.PlaceholderText = "(multiple)";
            }
        }
    }
}
