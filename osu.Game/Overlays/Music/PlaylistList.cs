// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private readonly FillFlowContainer<PlaylistItem> items;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select(item => new PlaylistItem(item) { OnSelect = itemSelected }).ToList();
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingCurrentFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        public Action<BeatmapSetInfo> OnSelect;

        private readonly SearchContainer search;

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
                new ScrollContainer
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
            public bool MatchingCurrentFilter
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
                LayoutEasing = EasingTypes.OutQuint;
            }
        }
    }
}