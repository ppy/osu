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
            Margin = new MarginPadding { Top = 5 }
        };

        private sealed class NumberControl : CompositeDrawable, IHasCurrentValue<int?>
        {
            private readonly BindableWithCurrent<int?> current = new BindableWithCurrent<int?>();

            private readonly OutlinedNumberBox numberBox;

            public Bindable<int?> Current
            {
                get => current;
                set
                {
                    current.Current = value;
                    numberBox.Text = value.Value.ToString();
                }
            }

            public NumberControl()
            {
                AutoSizeAxes = Axes.Y;

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
                    int? value = null;

                    if (int.TryParse(e.NewValue, out var intVal))
                        value = intVal;

                    current.Value = value;
                });
            }

            protected override void Update()
            {
                base.Update();
                if (Current.Value == null)
                    numberBox.Current.Value = "";
            }
        }
    }
}
