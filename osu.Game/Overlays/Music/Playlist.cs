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
    public class Playlist : RearrangeableListContainer<BeatmapSetInfo>
    {
        public Action<BeatmapSetInfo> RequestSelection;

        public readonly Bindable<BeatmapSetInfo> SelectedSet = new Bindable<BeatmapSetInfo>();

        /// <summary>
        /// Whether any item is currently being dragged. Used to hide other items' drag handles.
        /// </summary>
        private readonly BindableBool playlistDragActive = new BindableBool();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public void Filter(string searchTerm) => ((SearchContainer<RearrangeableListItem<BeatmapSetInfo>>)ListContainer).SearchTerm = searchTerm;

        public BeatmapSetInfo FirstVisibleSet => Items.FirstOrDefault(i => ((PlaylistItem)ItemMap[i]).MatchingFilter);

        protected override RearrangeableListItem<BeatmapSetInfo> CreateDrawable(BeatmapSetInfo item) => new PlaylistItem(item)
        {
            SelectedSet = { BindTarget = SelectedSet },
            PlaylistDragActive = { BindTarget = playlistDragActive },
            RequestSelection = set => RequestSelection?.Invoke(set)
        };

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override FillFlowContainer<RearrangeableListItem<BeatmapSetInfo>> CreateListFillFlowContainer() => new SearchContainer<RearrangeableListItem<BeatmapSetInfo>>
        {
            Spacing = new Vector2(0, 3),
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
        };
    }
}
