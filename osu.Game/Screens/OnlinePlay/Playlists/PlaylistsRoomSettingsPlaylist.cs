// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    /// <summary>
    /// A <see cref="DrawableRoomPlaylist"/> which is displayed during the setup stage of a playlists room.
    /// </summary>
    public class PlaylistsRoomSettingsPlaylist : DrawableRoomPlaylist
    {
        public PlaylistsRoomSettingsPlaylist()
        {
            AllowReordering = true;
            AllowDeletion = true;

            RequestDeletion = item =>
            {
                var nextItem = Items.GetNext(item);

                Items.Remove(item);

                SelectedItem.Value = nextItem ?? Items.LastOrDefault();
            };
        }
    }
}
