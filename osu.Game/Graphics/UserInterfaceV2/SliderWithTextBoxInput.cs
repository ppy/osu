// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Numerics;
using System.Globalization;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Utils;
using Vector2 = osuTK.Vector2;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class SliderWithTextBoxInput<T> : CompositeDrawable, IHasCurrentValue<T>
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => slider.KeyboardStep;
            set => slider.KeyboardStep = value;
        }

        private readonly BindableNumberWithCurrent<T> current = new BindableNumberWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current;
            set
            {
                current.Current = value;
                slider.Current = new BindableNumber<T>
                {
                    MinValue = current.MinValue,
                    MaxValue = current.MaxValue,
                    Default = current.Default,
                    Precision = sliderPrecision ?? current.Precision,
                };
            }
        }

        private T? sliderPrecision;

        public T? SliderPrecision
        {
            get => sliderPrecision;
            set
            {
                if (value.HasValue)
                {
                    T multiple = value.Value / current.Precision;
                    if (!T.IsNaN(multiple) && !T.IsInfinity(multiple) && !T.IsZero(multiple % T.One))
                        throw new ArgumentException(@"Precision override must be a multiple of the current precision.");
                }

                sliderPrecision = value;
                slider.Current = new BindableNumber<T>
                {
                    MinValue = current.MinValue,
                    MaxValue = current.MaxValue,
                    Default = current.Default,
                    Precision = value ?? current.Precision,
                };
            }
        }

        private bool instantaneous;

        /// <summary>
        /// Whether changes to the slider should instantaneously transfer to the text box (and vice versa).
        /// If <see langword="false"/>, the transfer will happen on text box commit (explicit, or implicit via focus loss), or on slider drag end.
        /// </summary>
        public bool Instantaneous
        {
            get => instantaneous;
            set
            {
                instantaneous = value;
                slider.TransferValueOnCommit = !instantaneous;
            }
        }

        private readonly SettingsSlider<T> slider;
        private readonly LabelledTextBox textBox;

        public SliderWithTextBoxInput(LocalisableString labelText)
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
                    Spacing = new Vector2(20),
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

            textBox.OnCommit += textCommitted;
            textBox.Current.BindValueChanged(textChanged);

            slider.Current.BindValueChanged(updateCurrentFromSlider);
            Current.BindValueChanged(updateTextBoxAndSliderFromCurrent, true);
        }

        public bool TakeFocus() => GetContainingFocusManager()?.ChangeFocus(textBox) == true;

        public bool SelectAll() => textBox.SelectAll();

        private bool updatingFromCurrent;
        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            if (!instantaneous) return;

            tryUpdateCurrentFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateCurrentFromTextBox();

            // If the attempted update above failed, restore text box to match the slider.
            Current.TriggerChange();
        }

        private void tryUpdateCurrentFromTextBox()
        {
            if (updatingFromCurrent) return;

            updatingFromTextBox = true;

            try
            {
                switch (Current)
                {
                    case Bindable<int> bindableInt:
                        bindableInt.Value = int.Parse(textBox.Current.Value);
                        break;

                    case Bindable<double> bindableDouble:
                        bindableDouble.Value = double.Parse(textBox.Current.Value);
                        break;

                    default:
                        Current.Parse(textBox.Current.Value, CultureInfo.CurrentCulture);
                        break;
                }
            }
            catch
            {
                // ignore parsing failures.
                // sane state will eventually be restored by a commit (either explicit, or implicit via focus loss).
            }

            updatingFromTextBox = false;
        }

        private void updateCurrentFromSlider(ValueChangedEvent<T> _)
        {
            if (updatingFromCurrent) return;

            Current.Value = slider.Current.Value;
        }

        private void updateTextBoxAndSliderFromCurrent(ValueChangedEvent<T> _)
        {
            updatingFromCurrent = true;

            slider.Current.Value = Current.Value;

            if (!updatingFromTextBox)
            {
                decimal decimalValue = decimal.CreateTruncating(Current.Value);
                textBox.Text = decimalValue.ToString($@"N{FormatUtils.FindPrecision(decimalValue)}");
            }

            updatingFromCurrent = false;
        }
    }
}
