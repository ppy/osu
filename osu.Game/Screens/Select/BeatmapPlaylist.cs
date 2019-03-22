// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : CompositeDrawable
    {
        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();
        private readonly FillFlowContainer<BeatmapPlaylistItem> playlistFlowContainer;

        public BeatmapPlaylist()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarOverlapsContent = false,
                Padding = new MarginPadding(5),
                Child = playlistFlowContainer = new FillFlowContainer<BeatmapPlaylistItem>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(1),
                }
            };

            playlist.ItemsAdded += itemsAdded;
        }

        private void itemsAdded(IEnumerable<PlaylistItem> items)
        {
            foreach (var item in items)
            {
                var drawable = new BeatmapPlaylistItem(item);
                drawable.RequestRemoval += handleRemoval;
                playlistFlowContainer.Add(drawable);
            }
        }

        private void handleRemoval(BeatmapPlaylistItem item)
        {
            playlist.Remove(item.PlaylistItem.Value);
            playlistFlowContainer.Remove(item);
        }

        public void AddItem(PlaylistItem item)
        {
            playlist.Add(item);
        }
    }
}
