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

namespace osu.Game.Screens.Edit.Timing
{
    public class SliderWithTextBoxInput<T> : CompositeDrawable, IHasCurrentValue<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        private readonly SettingsSlider<T> slider;

        public SliderWithTextBoxInput(LocalisableString labelText)
        {
            LabelledTextBox textBox;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        textBox = new LabelledTextBox
                        {
                            Label = labelText,
                        },
                        slider = new SettingsSlider<T>
                        {
                            TransferValueOnCommit = true,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                },
            };

            textBox.OnCommit += (t, isNew) =>
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

            Current.BindValueChanged(val =>
            {
                decimal decimalValue = slider.Current.Value.ToDecimal(NumberFormatInfo.InvariantInfo);
                textBox.Text = decimalValue.ToString($@"N{FormatUtils.FindPrecision(decimalValue)}");
            }, true);
        }

        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => slider.KeyboardStep;
            set => slider.KeyboardStep = value;
        }

        public Bindable<T> Current
        {
            get => slider.Current;
            set => slider.Current = value;
        }
    }
}
