// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Music
{
    public class Playlist : OsuRearrangeableListContainer<BeatmapSetInfo>
    {
        public Action<BeatmapSetInfo> RequestSelection;

        public readonly Bindable<BeatmapSetInfo> SelectedSet = new Bindable<BeatmapSetInfo>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public void Filter(FilterCriteria criteria)
        {
            var items = (SearchContainer<RearrangeableListItem<BeatmapSetInfo>>)ListContainer;

            foreach (var item in items.OfType<PlaylistItem>())
                item.InSelectedCollection = criteria.Collection?.Beatmaps.Any(b => item.Model.Equals(b.BeatmapSet)) ?? true;

            items.SearchTerm = criteria.SearchText;
        }

        public BeatmapSetInfo FirstVisibleSet => Items.FirstOrDefault(i => ((PlaylistItem)ItemMap[i]).MatchingFilter);

        protected override OsuRearrangeableListItem<BeatmapSetInfo> CreateOsuDrawable(BeatmapSetInfo item) => new PlaylistItem(item)
        {
            SelectedSet = { BindTarget = SelectedSet },
            RequestSelection = set => RequestSelection?.Invoke(set)
        };

        protected override FillFlowContainer<RearrangeableListItem<BeatmapSetInfo>> CreateListFillFlowContainer() => new SearchContainer<RearrangeableListItem<BeatmapSetInfo>>
        {
            Spacing = new Vector2(0, 3),
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
        };
    }
}
