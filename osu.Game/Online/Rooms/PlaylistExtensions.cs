// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Humanizer.Localisation;
using osu.Framework.Bindables;

namespace osu.Game.Online.Rooms
{
    public static class PlaylistExtensions
    {
        /// <summary>
        /// Returns the first non-expired <see cref="PlaylistItem"/> in playlist order from the supplied <paramref name="playlist"/>,
        /// or the last-played <see cref="PlaylistItem"/> if all items are expired,
        /// or <see langword="null"/> if <paramref name="playlist"/> was empty.
        /// </summary>
        public static PlaylistItem? GetCurrentItem(this ICollection<PlaylistItem> playlist)
        {
            if (playlist.Count == 0)
                return null;

            return playlist.All(item => item.Expired)
                ? playlist.OrderByDescending(item => item.PlaylistOrder).First()
                : playlist.OrderBy(item => item.PlaylistOrder).First(item => !item.Expired);
        }

        public static string GetTotalDuration(this BindableList<PlaylistItem> playlist) =>
            playlist.Select(p => p.Beatmap.Value.Length).Sum().Milliseconds().Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, precision: 2);
    }
}
