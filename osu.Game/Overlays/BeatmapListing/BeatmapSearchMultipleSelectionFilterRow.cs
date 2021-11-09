// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchMultipleSelectionFilterRow<T> : BeatmapSearchFilterRow<List<T>>
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
            Current.BindTo(filter.Current);
        }

        protected sealed override Drawable CreateFilter() => filter = CreateMultipleSelectionFilter();

        /// <summary>
        /// Creates a filter control that can be used to simultaneously select multiple values of type <typeparamref name="T"/>.
        /// </summary>
        [NotNull]
        protected virtual MultipleSelectionFilter CreateMultipleSelectionFilter() => new MultipleSelectionFilter();

        protected class MultipleSelectionFilter : FillFlowContainer<MultipleSelectionFilterTabItem>
        {
            public readonly BindableList<T> Current = new BindableList<T>();

            [BackgroundDependencyLoader]
            private void load()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                RelativeSizeAxes = Axes.X;
                Height = 15;
                Spacing = new Vector2(10, 0);

                AddRange(GetValues().Select(CreateTabItem));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var item in Children)
                    item.Active.BindValueChanged(active => toggleItem(item.Value, active.NewValue));
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
                    Current.Add(value);
                else
                    Current.Remove(value);
            }
        }

        protected class MultipleSelectionFilterTabItem : FilterTabItem<T>
        {
            public MultipleSelectionFilterTabItem(T value)
                : base(value)
            {
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
