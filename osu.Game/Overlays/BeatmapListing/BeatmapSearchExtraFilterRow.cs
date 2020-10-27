// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchExtraFilterRow : BeatmapSearchFilterRow<SearchExtra>
    {
        public BeatmapSearchExtraFilterRow()
            : base("Extra")
        {
        }

        protected override Drawable CreateFilter() => new ExtraFilter();

        private class ExtraFilter : FillFlowContainer<ExtraFilterTabItem>, IHasCurrentValue<SearchExtra>
        {
            private readonly BindableWithCurrent<SearchExtra> current = new BindableWithCurrent<SearchExtra>();

            public Bindable<SearchExtra> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private readonly ExtraFilterTabItem videoItem;
            private readonly ExtraFilterTabItem storyboardItem;

            public ExtraFilter()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                RelativeSizeAxes = Axes.X;
                Height = 15;
                Spacing = new Vector2(10, 0);
                AddRange(new[]
                {
                    videoItem = new ExtraFilterTabItem(SearchExtra.Video),
                    storyboardItem = new ExtraFilterTabItem(SearchExtra.Storyboard)
                });

                foreach (var item in Children)
                    item.StateUpdated += updateBindable;
            }

            private void updateBindable()
            {
                if (videoItem.Active.Value && storyboardItem.Active.Value)
                {
                    Current.Value = SearchExtra.Both;
                    return;
                }

                if (videoItem.Active.Value)
                {
                    Current.Value = SearchExtra.Video;
                    return;
                }

                if (storyboardItem.Active.Value)
                {
                    Current.Value = SearchExtra.Storyboard;
                    return;
                }

                Current.Value = SearchExtra.Any;
            }
        }

        private class ExtraFilterTabItem : FilterTabItem
        {
            public event Action StateUpdated;

            public ExtraFilterTabItem(SearchExtra value)
                : base(value)
            {
                Active.BindValueChanged(_ => StateUpdated?.Invoke());
            }

            protected override bool OnClick(ClickEvent e)
            {
                base.OnClick(e);
                Active.Value = !Active.Value;
                return true;
            }

            protected override string CreateText(SearchExtra value) => $@"Has {value.ToString()}";
        }
    }
}
