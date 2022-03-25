// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Music
{
    public class Playlist : OsuRearrangeableListContainer<Live<BeatmapSetInfo>>
    {
        public Action<Live<BeatmapSetInfo>> RequestSelection;

        public readonly Bindable<Live<BeatmapSetInfo>> SelectedSet = new Bindable<Live<BeatmapSetInfo>>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public void Filter(FilterCriteria criteria)
        {
            var items = (SearchContainer<RearrangeableListItem<Live<BeatmapSetInfo>>>)ListContainer;

            foreach (var item in items.OfType<PlaylistItem>())
                item.InSelectedCollection = criteria.Collection?.Beatmaps.Any(b => item.Model.ID == b.BeatmapSet?.ID) ?? true;

            items.SearchTerm = criteria.SearchText;
        }

        public Live<BeatmapSetInfo> FirstVisibleSet => Items.FirstOrDefault(i => ((PlaylistItem)ItemMap[i]).MatchingFilter);

        protected override OsuRearrangeableListItem<Live<BeatmapSetInfo>> CreateOsuDrawable(Live<BeatmapSetInfo> item) => new PlaylistItem(item)
        {
            SelectedSet = { BindTarget = SelectedSet },
            RequestSelection = set => RequestSelection?.Invoke(set)
        };

        protected override FillFlowContainer<RearrangeableListItem<Live<BeatmapSetInfo>>> CreateListFillFlowContainer() => new SearchContainer<RearrangeableListItem<Live<BeatmapSetInfo>>>
        {
            Spacing = new Vector2(0, 3),
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
        };
    }
}
