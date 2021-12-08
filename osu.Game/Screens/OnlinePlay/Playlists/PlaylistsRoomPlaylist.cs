// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsRoomPlaylist : DrawableRoomPlaylist
    {
        public PlaylistsRoomPlaylist(bool allowEdit, bool allowSelection, bool showItemOwner = false)
            : base(allowEdit, allowSelection, showItemOwner)
        {
            DeletionRequested = item =>
            {
                var nextItem = Items.GetNext(item);

                Items.Remove(item);

                SelectedItem.Value = nextItem ?? Items.LastOrDefault();
            };
        }
    }
}
