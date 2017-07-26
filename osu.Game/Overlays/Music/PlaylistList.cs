// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private FillFlowContainer<PlaylistItem> items;

        public Action<BeatmapSetInfo> OnSelect;

        public Action<IList<BeatmapSetInfo>> ReorderList;

        private readonly SearchContainer search;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select(item => new PlaylistItem(item) { OnSelect = itemSelected, OnReorder = reorderList }).ToList();
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        private void reorderList(PlaylistItem item)
        {
            IList<BeatmapSetInfo> currentList = new List<BeatmapSetInfo>();

            if (item.Position.Y > items.ElementAt(items.Count - 1).Position.Y)
            {
                items.Remove(item);
                items.Add(item);
            }

            for (int ctr = 0; ctr < items.Count; ctr++)
            {
                if (items.ElementAt(ctr).IsHovered && items.ElementAt(ctr) != item)
                {
                    PlaylistItem tempItem;
                    FillFlowContainer<PlaylistItem> tempItems = new FillFlowContainer<PlaylistItem>();

                    items.Remove(item);
                    tempItems.Add(item);

                    for (int innerCtr = ctr; innerCtr < items.Count; innerCtr++)
                    {
                        tempItem = items.ElementAt(ctr);
                        items.Remove(tempItem);
                        tempItems.Add(tempItem);
                        innerCtr--;                         //Needed because items.Count decreases with every items.Remove()
                    }

                    for (int innerCtr = 0; innerCtr < tempItems.Count; innerCtr++)
                    {
                        tempItem = tempItems.ElementAt(innerCtr);
                        tempItems.Remove(tempItem);
                        items.Add(tempItem);
                        innerCtr--;                         //Needed because items.Count decreases with every items.Remove()
                    }

                    break;
                }

            }

            foreach (var playlistItem in items)
                currentList.Add(playlistItem.BeatmapSetInfo);

            ReorderList.Invoke(currentList);
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