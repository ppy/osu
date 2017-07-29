// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private readonly PlaylistListContainer items;

        public Action<BeatmapSetInfo> OnSelect;

        public Action<IList<PlaylistItem>> ReorderList;

        private readonly SearchContainer search;

        public IList<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.AddRange(value.Select(item => new PlaylistItem(item) {
                    OnSelect = itemSelected,
                    DragStart = items.OnChildDragStart,
                    Drag = items.OnChildDrag,
                    DragEnd = items.OnChildDragEnd }));
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(PlaylistItem b)
        {
            OnSelect?.Invoke(b.BeatmapSetInfo);
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
                                items = new PlaylistListContainer
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
            items.Add(new PlaylistItem(beatmapSet) {
                OnSelect = itemSelected,
                DragStart = items.OnChildDragStart,
                Drag = items.OnChildDrag,
                DragEnd = items.OnChildDragEnd });
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            PlaylistItem itemToRemove = items.Children.FirstOrDefault(item => item.BeatmapSetInfo == beatmapSet);
            if (itemToRemove != null) items.Remove(itemToRemove);
        }

        private class PlaylistListContainer : FillFlowContainer<PlaylistItem>, IHasFilterableChildren, IHasDraggableChildren<PlaylistItem>
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

            public PlaylistListContainer()
            {
                LayoutDuration = 200;
                LayoutEasing = Easing.OutQuint;
                privateList = new PlaylistLinkedList();
            }

            public Action<IList<PlaylistItem>> ReorderList;

            private bool isItemBeingDragged;

            private PlaylistItem itemBeingDragged;

            private InputState stateDuringDrag;

            private PlaylistLinkedList privateList;

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
                reorderList(child as PlaylistItem, state);
                return;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (isItemBeingDragged)
                    itemBeingDragged.MoveToY(stateDuringDrag.Mouse.Position.Y);

                return;
            }

            private void reorderList(PlaylistItem item, InputState state)
            {
                PlaylistItem pivot;
                bool addToEnd = false;
                int newPosition = (int) Math.Round(state.Mouse.Position.Y / item.Height);

                if (newPosition >= privateList.Count)
                {
                    pivot = privateList[privateList.Count - 1];
                    addToEnd = true;
                }
                else if (newPosition <= 0)
                {
                    pivot = privateList[0];
                }
                else
                    pivot = privateList[newPosition];

                privateList.Remove(item);

                if (addToEnd)
                    privateList.AddAfter(privateList.Find(pivot), item);
                else
                    privateList.AddBefore(privateList.Find(pivot), item);

                UpdateChildren(privateList);
            }

            public override void Add(PlaylistItem drawable)
            {
                privateList.AddLast(drawable);
                UpdateChildren(drawable);
            }

            private void UpdateChildren(PlaylistItem item)
            {
                if (base.IndexOfInternal(item) > 0)
                    base.Remove(item);
                base.AddInternal(item);
            }

            private void UpdateChildren(PlaylistLinkedList list)
            {
                base.RemoveRange(list);
                base.AddRangeInternal(list.ToList());
            }

            private class PlaylistLinkedList : LinkedList<PlaylistItem>, IReadOnlyList<PlaylistItem>
            {
                public PlaylistItem this[int index]
                {
                    get
                    {
                        return this.ElementAt(index);
                    }
                }
            }
        }
    }
}