// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : CompositeDrawable
    {
        public Action<BeatmapSetInfo> OnSelect;

        private readonly ItemsScrollContainer items;

        public PlaylistList()
        {
            InternalChild = items = new ItemsScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                OnSelect = set => OnSelect?.Invoke(set)
            };
        }

        public new MarginPadding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        public IEnumerable<BeatmapSetInfo> BeatmapSets { set { items.Sets = value; } }

        public BeatmapSetInfo FirstVisibleSet => items.FirstVisibleSet;
        public BeatmapSetInfo NextSet => items.NextSet;
        public BeatmapSetInfo PreviousSet => items.PreviousSet;

        public BeatmapSetInfo SelectedSet
        {
            get { return items.SelectedSet; }
            set { items.SelectedSet = value; }
        }

        public void AddBeatmapSet(BeatmapSetInfo beatmapSet) => items.AddBeatmapSet(beatmapSet);
        public bool RemoveBeatmapSet(BeatmapSetInfo beatmapSet) => items.RemoveBeatmapSet(beatmapSet);

        public void Filter(string searchTerm) => items.SearchTerm = searchTerm;

        private class ItemsScrollContainer : OsuScrollContainer
        {
            public Action<BeatmapSetInfo> OnSelect;

            private readonly SearchContainer search;
            private readonly FillFlowContainer<PlaylistItem> items;

            private PlaylistItem draggedItem;

            public ItemsScrollContainer()
            {
                Children = new Drawable[]
                {
                    search = new SearchContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            items = new ItemSearchContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            },
                        }
                    }
                };
            }

            public IEnumerable<BeatmapSetInfo> Sets
            {
                set
                {
                    items.Clear();
                    value.ForEach(AddBeatmapSet);
                }
            }

            public string SearchTerm
            {
                get { return search.SearchTerm; }
                set { search.SearchTerm = value; }
            }

            public void AddBeatmapSet(BeatmapSetInfo beatmapSet)
            {
                items.Add(new PlaylistItem(beatmapSet)
                {
                    OnSelect = set => OnSelect?.Invoke(set),
                    Depth = items.Count
                });
            }

            public bool RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
            {
                var itemToRemove = items.FirstOrDefault(i => i.BeatmapSetInfo.ID == beatmapSet.ID);
                if (itemToRemove == null)
                    return false;
                return items.Remove(itemToRemove);
            }

            public BeatmapSetInfo SelectedSet
            {
                get { return items.FirstOrDefault(i => i.Selected)?.BeatmapSetInfo; }
                set
                {
                    foreach (PlaylistItem s in items.Children)
                        s.Selected = s.BeatmapSetInfo.ID == value?.ID;
                }
            }

            public BeatmapSetInfo FirstVisibleSet => items.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;
            public BeatmapSetInfo NextSet => (items.SkipWhile(i => !i.Selected).Skip(1).FirstOrDefault() ?? items.FirstOrDefault())?.BeatmapSetInfo;
            public BeatmapSetInfo PreviousSet => (items.TakeWhile(i => !i.Selected).LastOrDefault() ?? items.LastOrDefault())?.BeatmapSetInfo;

            protected override bool OnDragStart(InputState state)
            {
                draggedItem = items.FirstOrDefault(d => d.IsDraggable);
                return draggedItem != null || base.OnDragStart(state);
            }

            protected override bool OnDrag(InputState state)
            {
                if (draggedItem == null)
                    return base.OnDrag(state);

                // Mouse position in the position space of the items container
                Vector2 itemsPos = items.ToLocalSpace(state.Mouse.NativeState.Position);

                int src = (int)draggedItem.Depth;

                // Find the last item with position < mouse position. Note we can't directly use
                // the item positions as they are being transformed
                float heightAccumulator = 0;
                int dst = 0;
                for (; dst < items.Count; dst++)
                {
                    // Using BoundingBox here takes care of scale, paddings, etc...
                    heightAccumulator += items[dst].BoundingBox.Height;
                    if (heightAccumulator > itemsPos.Y)
                        break;
                }

                dst = MathHelper.Clamp(dst, 0, items.Count - 1);

                if (src == dst)
                    return true;

                if (src < dst)
                {
                    for (int i = src + 1; i <= dst; i++)
                        items.ChangeChildDepth(items[i], i - 1);
                }
                else
                {
                    for (int i = dst; i < src; i++)
                        items.ChangeChildDepth(items[i], i + 1);
                }

                items.ChangeChildDepth(draggedItem, dst);

                return true;
            }

            protected override bool OnDragEnd(InputState state) => draggedItem != null || base.OnDragEnd(state);

            private class ItemSearchContainer : FillFlowContainer<PlaylistItem>, IHasFilterableChildren
            {
                public string[] FilterTerms => new string[] { };
                public bool MatchingFilter
                {
                    set
                    {
                        if (value)
                            InvalidateLayout();
                    }
                }

                // Compare with reversed ChildID and Depth
                protected override int Compare(Drawable x, Drawable y) => base.Compare(y, x);

                public IEnumerable<IFilterable> FilterableChildren => Children;

                public ItemSearchContainer()
                {
                    LayoutDuration = 200;
                    LayoutEasing = Easing.OutQuint;
                }
            }
        }
    }
}