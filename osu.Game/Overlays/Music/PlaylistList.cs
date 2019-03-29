// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Music
{
    public class PlaylistList : CompositeDrawable
    {
        public Action<BeatmapSetInfo> Selected;
        public Action<BeatmapSetInfo, int> OrderChanged;

        private readonly ItemsScrollContainer items;

        public PlaylistList()
        {
            InternalChild = items = new ItemsScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Selected = set => Selected?.Invoke(set),
                OrderChanged = (s, i) => OrderChanged?.Invoke(s, i)
            };
        }

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public BeatmapSetInfo FirstVisibleSet => items.FirstVisibleSet;

        public void Filter(string searchTerm) => items.SearchTerm = searchTerm;

        private class ItemsScrollContainer : OsuScrollContainer
        {
            public Action<BeatmapSetInfo> Selected;
            public Action<BeatmapSetInfo, int> OrderChanged;

            private readonly SearchContainer search;
            private readonly FillFlowContainer<PlaylistItem> items;

            private readonly IBindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

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

            [BackgroundDependencyLoader]
            private void load(BeatmapManager beatmaps, IBindable<WorkingBeatmap> beatmap)
            {
                beatmaps.GetAllUsableBeatmapSets().ForEach(b => addBeatmapSet(b, false));
                beatmaps.ItemAdded += addBeatmapSet;
                beatmaps.ItemRemoved += removeBeatmapSet;

                beatmapBacking.BindTo(beatmap);
                beatmapBacking.ValueChanged += _ => updateSelectedSet();
            }

            private void addBeatmapSet(BeatmapSetInfo obj, bool existing) => Schedule(() =>
            {
                if (existing)
                    return;

                var newItem = new PlaylistItem(obj) { OnSelect = set => Selected?.Invoke(set) };

                items.Add(newItem);
                items.SetLayoutPosition(newItem, items.Count - 1);
            });

            private void removeBeatmapSet(BeatmapSetInfo obj) => Schedule(() =>
            {
                var itemToRemove = items.FirstOrDefault(i => i.BeatmapSetInfo.ID == obj.ID);
                if (itemToRemove != null)
                    items.Remove(itemToRemove);
            });

            private void updateSelectedSet()
            {
                foreach (PlaylistItem s in items.Children)
                    s.Selected = s.BeatmapSetInfo.ID == beatmapBacking.Value.BeatmapSetInfo?.ID;
            }

            public string SearchTerm
            {
                get => search.SearchTerm;
                set => search.SearchTerm = value;
            }

            public BeatmapSetInfo FirstVisibleSet => items.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

            private Vector2 nativeDragPosition;
            private PlaylistItem draggedItem;

            protected override bool OnDragStart(DragStartEvent e)
            {
                nativeDragPosition = e.ScreenSpaceMousePosition;
                draggedItem = items.FirstOrDefault(d => d.IsDraggable);
                return draggedItem != null || base.OnDragStart(e);
            }

            protected override bool OnDrag(DragEvent e)
            {
                nativeDragPosition = e.ScreenSpaceMousePosition;
                if (draggedItem == null)
                    return base.OnDrag(e);

                return true;
            }

            protected override bool OnDragEnd(DragEndEvent e)
            {
                nativeDragPosition = e.ScreenSpaceMousePosition;
                var handled = draggedItem != null || base.OnDragEnd(e);
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
                OrderChanged?.Invoke(draggedItem.BeatmapSetInfo, dstIndex);
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

                public bool FilteringActive
                {
                    set { }
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
