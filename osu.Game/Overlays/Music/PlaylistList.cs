// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private const float itemSpacing = 22f;

        private readonly ItemSearchContainer items;

        public Action<BeatmapSetInfo> OnSelect;

        public Action<IList<PlaylistItem>> ReorderList;

        private readonly SearchContainer search;

        public IList<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select(item => new PlaylistItem(item) { OnSelect = itemSelected, OnReorder = reorderList }).ToList();

                for (int ctr = 0; ctr < items.Count; ctr++)
                    items.ChangeChildDepth(items.ElementAt(ctr), ctr);
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        private void reorderList(PlaylistItem item)
        {
            bool alreadySorted = false;

            if (item.Position.Y > items.ElementAt(items.Count - 1).Position.Y)
            {
                int index = items.IndexOf(item);
                items.Remove(item);
                items.Add(item);

                for (int ctr = index; ctr < items.Count; ctr++)
                    items.ElementAt(ctr).Position = new OpenTK.Vector2(0, items.ElementAt(ctr - 1).Position.Y + itemSpacing);

                alreadySorted = true;
            }

            float itemPosition = item.Position.Y;
            PlaylistItem tempItem;
            ScrollContainer<PlaylistItem> tempItems = new ScrollContainer<PlaylistItem>();

            for (int ctr = 0; ctr < items.Count; ctr++)
            {
                if (alreadySorted)
                    break;

                if (items.ElementAt(ctr).Position.Y >= itemPosition)
                {
                    if (!tempItems.Contains(item))
                    {
                        item.Position = new OpenTK.Vector2(0, items.ElementAt(ctr).Position.Y);
                        items.Remove(item);
                        tempItems.Add(item);
                    }

                    tempItem = items.ElementAt(ctr);
                    tempItem.Position = new OpenTK.Vector2(0, tempItem.Position.Y + itemSpacing);
                    items.Remove(tempItem);
                    tempItems.Add(tempItem);

                    ctr--;
                }
            }

            for (int ctr = 0; ctr < tempItems.Count; ctr++)
            {
                tempItem = tempItems.ElementAt(ctr);
                tempItems.Remove(tempItem);
                items.Add(tempItem);
                ctr--;
            }

            ReorderList.Invoke(items.Children.ToList());
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

            public IEnumerable<IFilterable> FilterableChildren => Children;

            public ItemSearchContainer()
            {
                LayoutDuration = 200;
                LayoutEasing = Easing.OutQuint;
            }
        }
    }
}