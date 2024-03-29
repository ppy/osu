// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapSearchMultipleSelectionFilterRow<T> : BeatmapSearchFilterRow<List<T>>
        where T : Enum
    {
        public new readonly BindableList<T> Current = new BindableList<T>();

        private MultipleSelectionFilter filter;

        public BeatmapSearchMultipleSelectionFilterRow(LocalisableString header)
            : base(header)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            filter.Current.BindTo(Current);
        }

        protected sealed override Drawable CreateFilter() => filter = CreateMultipleSelectionFilter();

        /// <summary>
        /// Creates a filter control that can be used to simultaneously select multiple values of type <typeparamref name="T"/>.
        /// </summary>
        [NotNull]
        protected virtual MultipleSelectionFilter CreateMultipleSelectionFilter() => new MultipleSelectionFilter();

        protected partial class MultipleSelectionFilter : FillFlowContainer<MultipleSelectionFilterTabItem>
        {
            public readonly BindableList<T> Current = new BindableList<T>();

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Spacing = new Vector2(10, 0);

                AddRange(GetValues().Select(CreateTabItem));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var item in Children)
                    item.Active.BindValueChanged(active => toggleItem(item.Value, active.NewValue));

                Current.BindCollectionChanged(currentChanged, true);
            }

            private void currentChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                foreach (var c in Children)
                    c.Active.Value = Current.Contains(c.Value);
            }

            /// <summary>
            /// Returns all values to be displayed in this filter row.
            /// </summary>
            protected virtual IEnumerable<T> GetValues() => Enum.GetValues(typeof(T)).Cast<T>();

            /// <summary>
            /// Creates a <see cref="MultipleSelectionFilterTabItem"/> representing the supplied <paramref name="value"/>.
            /// </summary>
            protected virtual MultipleSelectionFilterTabItem CreateTabItem(T value) => new MultipleSelectionFilterTabItem(value);

            private void toggleItem(T value, bool active)
            {
                if (active)
                {
                    if (!Current.Contains(value))
                        Current.Add(value);
                }
                else
                    Current.Remove(value);
            }
        }

        protected partial class MultipleSelectionFilterTabItem : FilterTabItem<T>
        {
            private readonly Box selectedUnderline;

            protected override bool HighlightOnHoverWhenActive => true;

            public MultipleSelectionFilterTabItem(T value)
                : base(value)
            {
                // This doesn't match any actual design, but should make it easier for the user to understand
                // that filters are applied until we settle on a final design.
                AddInternal(selectedUnderline = new Box
                {
                    Depth = float.MaxValue,
                    RelativeSizeAxes = Axes.X,
                    Height = 1.5f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                });
            }

            protected override HoverSounds CreateHoverSounds() => new HoverClickSounds(HoverSampleSet.TabSelect);

            protected override void UpdateState()
            {
                base.UpdateState();
                selectedUnderline.FadeTo(Active.Value ? 1 : 0, 200, Easing.OutQuint);
                selectedUnderline.FadeColour(IsHovered ? ColourProvider.Content2 : GetStateColour(), 200, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                base.OnClick(e);
                Active.Toggle();
                return true;
            }
        }
    }
}
