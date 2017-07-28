// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        protected readonly ItemSearchContainer items;

        public Action<BeatmapSetInfo> OnSelect;

        public Action<IList<PlaylistItem>> ReorderList;

        private readonly SearchContainer search;

        public IList<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select(item => new PlaylistItem(item) {
                    OnSelect = itemSelected,
                    DragStart = items.OnChildDragStart,
                    Drag = items.OnChildDrag,
                    DragEnd = items.OnChildDragEnd }).ToList();
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        public void Filter(string searchTerm) => search.SearchTerm = searchTerm;

        public BeatmapSetInfo SelectedItem
        {
            get { return items.Children.FirstOrDefault(i => i.Selected)?.BeatmapSetInfo; }
            set
            {
                foreach (PlaylistItem s in items.Children)
                    s.Selected = s.BeatmapSetInfo.ID == value?.ID;
            }
        }

        public PlaylistList()
        {
            Children = new Drawable[]
            {
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
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
                                    ReorderList = this.ReorderList
                                },
                            }
                        }
                    },
                },
            };
        }
        
        public void AddBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            items.Add(new PlaylistItem(beatmapSet) { OnSelect = itemSelected });
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            PlaylistItem itemToRemove = items.Children.FirstOrDefault(item => item.BeatmapSetInfo == beatmapSet);
            if (itemToRemove != null) items.Remove(itemToRemove);
        }

        protected class ItemSearchContainer : FillFlowContainer<PlaylistItem>, IHasFilterableChildren, IHasDraggableChildren
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

            public IEnumerable<IFilterable> FilterableChildren => Children;

            public ItemSearchContainer()
            {
                LayoutDuration = 200;
                LayoutEasing = Easing.OutQuint;
            }

            public Action<IList<PlaylistItem>> ReorderList;

            private bool isItemBeingDragged;

            private PlaylistItem itemBeingDragged;

            private InputState stateDuringDrag;

            public void OnChildDragStart(Drawable child, InputState state)
            {
                isItemBeingDragged = true;
                itemBeingDragged = child as PlaylistItem;
                stateDuringDrag = state;
                return;
            }

            public void OnChildDrag(Drawable child, InputState state)
            {
                stateDuringDrag = state;
                return;
            }

            public void OnChildDragEnd(Drawable child, InputState state)
            {
                isItemBeingDragged = false;
                itemBeingDragged = null;
                stateDuringDrag = null;
                reorderList(child as PlaylistItem);
                return;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (isItemBeingDragged)
                    itemBeingDragged.MoveToY(stateDuringDrag.Mouse.Position.Y);

                return;
            }

            private void reorderList(PlaylistItem item)
            {
                int newPosition = (int) Math.Floor(item.Position.Y / item.Height);
                int currentPosition = IndexOf(item);

                IList<PlaylistItem> newList = new List<PlaylistItem>();

                if (newPosition < currentPosition)
                    for (int ctr = 0; ctr < Children.Count; ctr++)
                    {
                        if (ctr == newPosition)
                        {
                            Remove(item);
                            newList.Add(item);
                        }

                        else
                        {
                            Remove(Children.ElementAt(ctr));
                            newList.Add(Children.ElementAt(ctr));
                        }

                        ctr--;
                    }

                else if (newPosition > currentPosition)
                {
                    for (int ctr = newPosition; ctr < Children.Count; ctr++)
                    { }
                }

                else
                    return;

                Children = newList.ToList();

                ReorderList.Invoke(newList);
            }
        }
    }
}