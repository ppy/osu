// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions;
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
                        Margin = new MarginPadding { Top = 5 },
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };

                numberBox.Current.BindValueChanged(e =>
                {
                    if (string.IsNullOrEmpty(e.NewValue))
                    {
                        Current.Value = null;
                        return;
                    }

                    if (int.TryParse(e.NewValue, out int intVal))
                        Current.Value = intVal;
                    else
                        numberBox.NotifyInputError();

                    // trigger Current again to either restore the previous text box value, or to reformat the new value via .ToString().
                    Current.TriggerChange();
                });

                Current.BindValueChanged(e =>
                {
                    numberBox.Current.Value = e.NewValue?.ToString();
                });
            }
        }

        private class OutlinedNumberBox : OutlinedTextBox
        {
            protected override bool AllowIme => false;

            protected override bool CanAddCharacter(char character) => character.IsAsciiDigit();

            public new void NotifyInputError() => base.NotifyInputError();
        }
    }
}
