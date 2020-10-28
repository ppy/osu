// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchMultipleSelectionFilterRow<T> : BeatmapSearchFilterRow<List<T>>
    {
        public new readonly BindableList<T> Current = new BindableList<T>();

        private MultipleSelectionFilter filter;

        public BeatmapSearchMultipleSelectionFilterRow(string headerName)
            : base(headerName)
        {
            Current.BindTo(filter.Current);
        }

        protected override Drawable CreateFilter() => filter = new MultipleSelectionFilter();

        private class MultipleSelectionFilter : FillFlowContainer<MultipleSelectionFilterTabItem>
        {
            public readonly BindableList<T> Current = new BindableList<T>();

            public MultipleSelectionFilter()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                RelativeSizeAxes = Axes.X;
                Height = 15;
                Spacing = new Vector2(10, 0);

                ((T[])Enum.GetValues(typeof(T))).ForEach(i => Add(new MultipleSelectionFilterTabItem(i)));

                foreach (var item in Children)
                    item.Active.BindValueChanged(active => updateBindable(item.Value, active.NewValue));
            }

            private void updateBindable(T value, bool active)
            {
                if (active)
                    Current.Add(value);
                else
                    Current.Remove(value);
            }
        }

        private class MultipleSelectionFilterTabItem : FilterTabItem<T>
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
