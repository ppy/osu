// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsNumberBox : SettingsItem<int?>
    {
        protected override Drawable CreateControl() => new NumberControl
        {
            RelativeSizeAxes = Axes.X,
        };

        private sealed class NumberControl : CompositeDrawable, IHasCurrentValue<int?>
        {
            private readonly BindableWithCurrent<int?> current = new BindableWithCurrent<int?>();

            public Bindable<int?> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public NumberControl()
            {
                AutoSizeAxes = Axes.Y;

                OutlinedNumberBox numberBox;

                InternalChildren = new[]
                {
                    numberBox = new OutlinedNumberBox
                    {
                        LengthLimit = 9, // limited to less than a value that could overflow int32 backing.
                        Margin = new MarginPadding { Top = 5 },
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };

                numberBox.Current.BindValueChanged(e =>
                {
                    int? value = null;

                    if (int.TryParse(e.NewValue, out int intVal))
                        value = intVal;

                    current.Value = value;
                });

                Current.BindValueChanged(e =>
                {
                    numberBox.Current.Value = e.NewValue?.ToString();
                });
            }
        }

        private class OutlinedNumberBox : OutlinedTextBox
        {
            protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        }
    }
}
