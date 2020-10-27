// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchMultipleSelectionFilterRow<T> : BeatmapSearchFilterRow<List<T>>
    {
        public BeatmapSearchMultipleSelectionFilterRow(string headerName)
            : base(headerName)
        {
        }

        protected override Drawable CreateFilter() => new MultipleSelectionFilter();

        private class MultipleSelectionFilter : FillFlowContainer<MultipleSelectionFilterTabItem>, IHasCurrentValue<List<T>>
        {
            private readonly BindableWithCurrent<List<T>> current = new BindableWithCurrent<List<T>>();

            public Bindable<List<T>> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public MultipleSelectionFilter()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                RelativeSizeAxes = Axes.X;
                Height = 15;
                Spacing = new Vector2(10, 0);

                ((T[])Enum.GetValues(typeof(T))).ForEach(i => Add(new MultipleSelectionFilterTabItem(i)));

                foreach (var item in Children)
                    item.Active.BindValueChanged(_ => updateBindable());
            }

            private void updateBindable()
            {
                var selectedValues = new List<T>();

                foreach (var item in Children)
                {
                    if (item.Active.Value)
                        selectedValues.Add(item.Value);
                }

                Current.Value = selectedValues;
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
