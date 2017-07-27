// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private readonly ItemSearchContainer items;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select(item => new PlaylistItem(item) { OnSelect = itemSelected }).ToList();
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        public Action<BeatmapSetInfo> OnSelect;

        public Action<IEnumerable<BeatmapSetInfo>> OnBeatmapSetsReorder;

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
                                    UpdateBeatmapSets = b => OnBeatmapSetsReorder?.Invoke(b),
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
            items.TriggerBeatmapSetsChange();
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            PlaylistItem itemToRemove = items.Children.FirstOrDefault(item => item.BeatmapSetInfo.Hash == beatmapSet.Hash);
            if (itemToRemove != null)
                items.Remove(itemToRemove);
            items.TriggerBeatmapSetsChange();
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

            public Action<List<BeatmapSetInfo>> UpdateBeatmapSets;
            private List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

            private Vector2 mousePosition;

            protected override bool OnMouseMove(InputState state)
            {
                mousePosition = state.Mouse.Position;
                return base.OnMouseMove(state);
            }

            protected override void Update()
            {
                base.Update();
                PlaylistItem item = Children.FirstOrDefault(i => i.DragStartOffset != Vector2.Zero);
                if (item == default(PlaylistItem))
                    return;
                List<PlaylistItem> childrenList = Children.ToList();
                item.MoveTo(new Vector2(0, (mousePosition - item.DragStartOffset).Y));

                int targetIndex = Children.Count(i => i.Position.Y - (item.Size.Y / 2f) < item.Position.Y - (i.Size.Y / 2f));

                if (targetIndex == childrenList.IndexOf(item))
                    return;
                bool hasFindItem = false;
                for (int i = 0; i < childrenList.Count; i++)
                {
                    if (childrenList[i] == item)
                    {
                        ChangeChildDepth(item, childrenList.Count - targetIndex);
                        hasFindItem = true;
                    }
                    else
                        ChangeChildDepth(childrenList[i], childrenList.Count - i + (hasFindItem ? 1 : -1));
                }
                TriggerBeatmapSetsChange();
            }

            public void TriggerBeatmapSetsChange()
            {
                beatmapSets = Children.Select(b => b.BeatmapSetInfo).ToList();
                UpdateBeatmapSets?.Invoke(beatmapSets);
            }
        }
    }
}