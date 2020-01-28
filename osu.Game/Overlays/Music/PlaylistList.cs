// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Music
{
    public class PlaylistList2 : BasicRearrangeableListContainer<PlaylistListItem>
    {
        public readonly BindableList<BeatmapSetInfo> BeatmapSets = new BindableList<BeatmapSetInfo>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BeatmapSets.ItemsAdded += addBeatmapSets;
            BeatmapSets.ItemsRemoved += removeBeatmapSets;
        }

        public void Filter(string searchTerm) => ((PlaylistListFlowContainer)ListContainer).SearchTerm = searchTerm;

        public BeatmapSetInfo FirstVisibleSet => ListContainer.FlowingChildren.Cast<DrawablePlaylistListItem>().FirstOrDefault(i => i.MatchingFilter)?.Model.BeatmapSetInfo;

        private void addBeatmapSets(IEnumerable<BeatmapSetInfo> sets) => Schedule(() =>
        {
            foreach (var item in sets)
                AddItem(new PlaylistListItem(item));
        });

        private void removeBeatmapSets(IEnumerable<BeatmapSetInfo> sets) => Schedule(() =>
        {
            foreach (var item in sets)
                RemoveItem(ListContainer.Children.Select(d => d.Model).FirstOrDefault(m => m.BeatmapSetInfo == item));
        });

        protected override BasicDrawableRearrangeableListItem CreateBasicItem(PlaylistListItem item) => new DrawablePlaylistListItem(item);

        protected override FillFlowContainer<DrawableRearrangeableListItem> CreateListFillFlowContainer() => new PlaylistListFlowContainer
        {
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint
        };
    }

    public class PlaylistListFlowContainer : SearchContainer<RearrangeableListContainer<PlaylistListItem>.DrawableRearrangeableListItem>
    {
    }

    public class PlaylistListItem : IEquatable<PlaylistListItem>
    {
        public readonly BeatmapSetInfo BeatmapSetInfo;

        public PlaylistListItem(BeatmapSetInfo beatmapSetInfo)
        {
            BeatmapSetInfo = beatmapSetInfo;
        }

        public override string ToString() => BeatmapSetInfo.ToString();

        public bool Equals(PlaylistListItem other) => BeatmapSetInfo.Equals(other?.BeatmapSetInfo);
    }

    public class DrawablePlaylistListItem : BasicRearrangeableListContainer<PlaylistListItem>.BasicDrawableRearrangeableListItem, IFilterable
    {
        public DrawablePlaylistListItem(PlaylistListItem item)
            : base(item)
        {
            FilterTerms = item.BeatmapSetInfo.Metadata.SearchableTerms;
        }

        public IEnumerable<string> FilterTerms { get; }

        private bool matching = true;

        public bool MatchingFilter
        {
            get => matching;
            set
            {
                if (matching == value) return;

                matching = value;

                this.FadeTo(matching ? 1 : 0, 200);
            }
        }

        public bool FilteringActive { get; set; }
    }
}
