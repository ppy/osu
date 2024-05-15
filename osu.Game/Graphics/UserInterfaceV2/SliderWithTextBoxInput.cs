// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public Bindable<T> Current
        {
            get => slider.Current;
            set => slider.Current = value;
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

            Current.BindValueChanged(updateTextBoxFromSlider, true);
        }

        public bool TakeFocus() => GetContainingInputManager().ChangeFocus(textBox);

        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            if (!instantaneous) return;

            tryUpdateSliderFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateSliderFromTextBox();

            // If the attempted update above failed, restore text box to match the slider.
            Current.TriggerChange();
        }

        private void tryUpdateSliderFromTextBox()
        {
            updatingFromTextBox = true;

            try
            {
                switch (slider.Current)
                {
                    case Bindable<int> bindableInt:
                        bindableInt.Value = int.Parse(textBox.Current.Value);
                        break;

                    case Bindable<double> bindableDouble:
                        bindableDouble.Value = double.Parse(textBox.Current.Value);
                        break;

                    default:
                        slider.Current.Parse(textBox.Current.Value, CultureInfo.CurrentCulture);
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

        private void updateTextBoxFromSlider(ValueChangedEvent<T> _)
        {
            if (updatingFromTextBox) return;

            decimal decimalValue = decimal.CreateTruncating(slider.Current.Value);
            textBox.Text = decimalValue.ToString($@"N{FormatUtils.FindPrecision(decimalValue)}");
        }
    }
}
