// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// A settings control for use by <see cref="IHasSeed"/> mods which have a customisable seed value.
    /// </summary>
    public class SeedSettingsControl : SettingsItem<int?>
    {
        protected override Drawable CreateControl() => new SeedControl
        {
            RelativeSizeAxes = Axes.X,
            Margin = new MarginPadding { Top = 5 }
        };

        private sealed class SeedControl : CompositeDrawable, IHasCurrentValue<int?>
        {
            private readonly BindableWithCurrent<int?> current = new BindableWithCurrent<int?>();

            public Bindable<int?> Current
            {
                get => current;
                set
                {
                    current.Current = value;
                    seedNumberBox.Text = value.Value.ToString();
                }
            }

            private readonly OutlinedNumberBox seedNumberBox;

            public SeedControl()
            {
                AutoSizeAxes = Axes.Y;

                InternalChildren = new[]
                {
                    seedNumberBox = new OutlinedNumberBox
                    {
                        Margin = new MarginPadding { Top = 5 },
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };

                seedNumberBox.Current.BindValueChanged(e =>
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
                    seedNumberBox.Current.Value = "";
            }
        }
    }
}
