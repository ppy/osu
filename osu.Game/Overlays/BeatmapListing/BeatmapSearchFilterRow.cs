// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using Humanizer;
using osu.Framework.Extensions.EnumExtensions;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchFilterRow<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public BeatmapSearchFilterRow(string headerName)
        {
            Drawable filter;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            AddInternal(new GridContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, size: 100),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.GetFont(size: 13),
                            Text = headerName.Titleize()
                        },
                        filter = CreateFilter()
                    }
                }
            });

            if (filter is IHasCurrentValue<T> filterWithValue)
                Current = filterWithValue.Current;
        }

        [NotNull]
        protected virtual Drawable CreateFilter() => new BeatmapSearchFilter();

        protected class BeatmapSearchFilter : TabControl<T>
        {
            public BeatmapSearchFilter()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                RelativeSizeAxes = Axes.X;
                Height = 15;

                TabContainer.Spacing = new Vector2(10, 0);

                if (typeof(T).IsEnum)
                {
                    foreach (var val in EnumExtensions.GetValuesInOrder<T>())
                        AddItem(val);
                }
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                if (Dropdown is FilterDropdown fd)
                    fd.AccentColour = colourProvider.Light2;
            }

            protected override Dropdown<T> CreateDropdown() => new FilterDropdown();

            protected override TabItem<T> CreateTabItem(T value) => new FilterTabItem<T>(value);

            private class FilterDropdown : OsuTabDropdown<T>
            {
                protected override DropdownHeader CreateHeader() => new FilterHeader
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                };

                private class FilterHeader : OsuTabDropdownHeader
                {
                    public FilterHeader()
                    {
                        Background.Height = 1;
                    }
                }
            }
        }
    }
}
