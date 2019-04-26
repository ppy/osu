// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylistItem : RearrangeableListItem
    {
        public PlaylistItem PlaylistItem;

        public BeatmapPlaylistItem(PlaylistItem playlistItem)
        {
            PlaylistItem = playlistItem;
        }
    }
}
