// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsRoomPlaylist : DrawableRoomPlaylist
    {
        private readonly bool allowReordering;
        private readonly bool allowDeletion;
        private readonly bool allowSelection;

        public PlaylistsRoomPlaylist(bool allowReordering, bool allowDeletion, bool allowSelection)
        {
            this.allowReordering = allowReordering;
            this.allowDeletion = allowDeletion;
            this.allowSelection = allowSelection;

            DeletionRequested = item =>
            {
                var nextItem = Items.GetNext(item);

                Items.Remove(item);

                SelectedItem.Value = nextItem ?? Items.LastOrDefault();
            };
        }

        protected override OsuRearrangeableListItem<PlaylistItem> CreateOsuDrawable(PlaylistItem item) => base.CreateOsuDrawable(item).With(d =>
        {
            var drawablePlaylistItem = (DrawableRoomPlaylistItem)d;

            drawablePlaylistItem.AllowReordering = allowReordering;
            drawablePlaylistItem.AllowDeletion = allowDeletion;
            drawablePlaylistItem.AllowSelection = allowSelection;
        });
    }
}
