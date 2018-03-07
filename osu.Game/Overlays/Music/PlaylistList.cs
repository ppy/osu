// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
    public class PlaylistList : CompositeDrawable
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

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get { return items.Sets; }
            set { items.Sets = value; }
        }

        public BeatmapSetInfo FirstVisibleSet => items.FirstVisibleSet;
        public BeatmapSetInfo NextSet => items.NextSet;
        public BeatmapSetInfo PreviousSet => items.PreviousSet;

        public BeatmapSetInfo SelectedSet
        {
            get { return items.SelectedSet; }
            set { items.SelectedSet = value; }
        }

        public void AddBeatmapSet(BeatmapSetInfo beatmapSet) => items.AddBeatmapSet(beatmapSet);
        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet) => items.RemoveBeatmapSet(beatmapSet);

        public void Filter(string searchTerm) => items.SearchTerm = searchTerm;

        private class ItemsScrollContainer : OsuScrollContainer
        {
            public Action<BeatmapSetInfo> OnSelect;

            private readonly SearchContainer search;
            private readonly FillFlowContainer<PlaylistItem> items;

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
                get { return items.Select(x => x.BeatmapSetInfo).ToList(); }
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
                var newItem = new PlaylistItem(beatmapSet) { OnSelect = set => OnSelect?.Invoke(set) };

                items.Add(newItem);
                items.SetLayoutPosition(newItem, items.Count);
            }

            public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
            {
                var itemToRemove = items.FirstOrDefault(i => i.BeatmapSetInfo.ID == beatmapSet.ID);
                if (itemToRemove != null)
                    items.Remove(itemToRemove);
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

            private Vector2 nativeDragPosition;
            private PlaylistItem draggedItem;

            protected override bool OnDragStart(InputState state)
            {
                nativeDragPosition = state.Mouse.NativeState.Position;
                draggedItem = items.FirstOrDefault(d => d.IsDraggable);
                return draggedItem != null || base.OnDragStart(state);
            }

            protected override bool OnDrag(InputState state)
            {
                nativeDragPosition = state.Mouse.NativeState.Position;
                if (draggedItem == null)
                    return base.OnDrag(state);
                return true;
            }

            protected override bool OnDragEnd(InputState state)
            {
                nativeDragPosition = state.Mouse.NativeState.Position;
                var handled = draggedItem != null || base.OnDragEnd(state);
                draggedItem = null;

                return handled;
            }

            protected override void Update()
            {
                base.Update();

                if (draggedItem == null)
                    return;

                updateScrollPosition();
                updateDragPosition();
            }

            private void updateScrollPosition()
            {
                const float start_offset = 10;
                const double max_power = 50;
                const double exp_base = 1.05;

                var localPos = ToLocalSpace(nativeDragPosition);

                if (localPos.Y < start_offset)
                {
                    if (Current <= 0)
                        return;

                    var power = Math.Min(max_power, Math.Abs(start_offset - localPos.Y));
                    ScrollBy(-(float)Math.Pow(exp_base, power));
                }
                else if (localPos.Y > DrawHeight - start_offset)
                {
                    if (IsScrolledToEnd())
                        return;

                    var power = Math.Min(max_power, Math.Abs(DrawHeight - start_offset - localPos.Y));
                    ScrollBy((float)Math.Pow(exp_base, power));
                }
            }

            private void updateDragPosition()
            {
                var itemsPos = items.ToLocalSpace(nativeDragPosition);

                int srcIndex = (int)items.GetLayoutPosition(draggedItem);

                // Find the last item with position < mouse position. Note we can't directly use
                // the item positions as they are being transformed
                float heightAccumulator = 0;
                int dstIndex = 0;
                for (; dstIndex < items.Count; dstIndex++)
                {
                    // Using BoundingBox here takes care of scale, paddings, etc...
                    heightAccumulator += items[dstIndex].BoundingBox.Height;
                    if (heightAccumulator > itemsPos.Y)
                        break;
                }

                dstIndex = MathHelper.Clamp(dstIndex, 0, items.Count - 1);

                if (srcIndex == dstIndex)
                    return;

                if (srcIndex < dstIndex)
                {
                    for (int i = srcIndex + 1; i <= dstIndex; i++)
                        items.SetLayoutPosition(items[i], i - 1);
                }
                else
                {
                    for (int i = dstIndex; i < srcIndex; i++)
                        items.SetLayoutPosition(items[i], i + 1);
                }

                items.SetLayoutPosition(draggedItem, dstIndex);
            }

            private class ItemSearchContainer : FillFlowContainer<PlaylistItem>, IHasFilterableChildren
            {
                public IEnumerable<string> FilterTerms => new string[] { };

                public bool MatchingFilter
                {
                    set
                    {
                        if (value)
                            InvalidateLayout();
                    }
                }

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
