// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
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

            private readonly OsuNumberBox seedNumberBox;

            public SeedControl()
            {
                AutoSizeAxes = Axes.Y;

                InternalChildren = new[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 2),
                            new Dimension(GridSizeMode.Relative, 0.25f)
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                seedNumberBox = new OsuNumberBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    CommitOnFocusLost = true
                                }
                            }
                        }
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
                if (current.Value == null)
                    seedNumberBox.Text = current.Current.Value.ToString();
            }
        }
    }
}
