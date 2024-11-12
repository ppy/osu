// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    public static class MultiplayerRoomExtensions
    {
        /// <summary>
        /// Returns all historical/expired items from the <paramref name="room"/>, in the order in which they were played.
        /// </summary>
        public static IEnumerable<MultiplayerPlaylistItem> GetHistoricalItems(this MultiplayerRoom room)
            => room.Playlist.Where(item => item.Expired).OrderBy(item => item.PlayedAt);

        /// <summary>
        /// Returns all non-expired items from the <paramref name="room"/>, in the order in which they are to be played.
        /// </summary>
        public static IEnumerable<MultiplayerPlaylistItem> GetUpcomingItems(this MultiplayerRoom room)
            => room.Playlist.Where(item => !item.Expired).OrderBy(item => item.PlaylistOrder);

        /// <summary>
        /// Returns the first non-expired <see cref="MultiplayerPlaylistItem"/> in playlist order from the supplied <paramref name="room"/>,
        /// or the last-played <see cref="MultiplayerPlaylistItem"/> if all items are expired,
        /// or <see langword="null"/> if <paramref name="room"/> was empty.
        /// </summary>
        public static MultiplayerPlaylistItem? GetCurrentItem(this MultiplayerRoom room)
        {
            if (room.Playlist.Count == 0)
                return null;

            return room.Playlist.All(item => item.Expired)
                ? GetHistoricalItems(room).Last()
                : GetUpcomingItems(room).First();
        }
    }
}
