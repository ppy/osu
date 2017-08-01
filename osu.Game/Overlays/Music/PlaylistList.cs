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
                items.AddRangeToList(value.Select(item => new PlaylistItem(item) {
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
                                    ReorderList = reorderList,
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
            PlaylistItem itemToRemove = items.Children.FirstOrDefault(item => item.BeatmapSetInfo.ID == beatmapSet.ID);
            if (itemToRemove != null) items.Remove(itemToRemove);
        }

        private void reorderList(IList<PlaylistItem> list)
        {
            ReorderList.Invoke(list);
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
                privateList = new SortedList<PlaylistItem>(Comparer<PlaylistItem>.Default);
            }

            public Action<IList<PlaylistItem>> ReorderList;

            private bool isItemBeingDragged;

            private PlaylistItem itemBeingDragged;

            private InputState stateDuringDrag;

            private SortedList<PlaylistItem> privateList;

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
                if (item == null)
                    return;

                int newPosition = (int)Math.Min(Math.Max(Math.Round(state.Mouse.Position.Y / item.Height), 0d), privateList.Count);
                int currentPosition = privateList.FindIndex(x => x.BeatmapSetInfo.Equals(item.BeatmapSetInfo));
                privateList.RemoveAt(currentPosition);

                //To account for the remove(item)
                if (newPosition > currentPosition)
                    newPosition--;
                
                if (newPosition == privateList.Count)
                    item.Position = new OpenTK.Vector2(item.Position.X, privateList.ElementAt(newPosition - 1).Position.Y + item.Height);
                else
                    item.Position = new OpenTK.Vector2(item.Position.X, privateList.ElementAt(newPosition).Position.Y);

                privateList.Add(item);

                updateChildren(privateList);
            }

            public void AddRangeToList(IEnumerable<PlaylistItem> range)
            {
                foreach (PlaylistItem item in range)
                    privateList.Add(item);
                updateChildren(privateList);
            }

            private void updateChildren(SortedList<PlaylistItem> list)
            {
                RemoveRange(list);
                AddRange(list);
                ReorderList.Invoke(list.ToList());
            }
        }
    }
}