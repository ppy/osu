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
        /// Returns the next <see cref="PlaylistItem"/> to be played from the supplied <paramref name="playlist"/>,
        /// or <see langword="null"/> if all items are expired.
        /// </summary>
        public static PlaylistItem? GetNextItem(this IEnumerable<PlaylistItem> playlist) =>
            playlist.OrderBy(item => item.PlaylistOrder).FirstOrDefault(item => !item.Expired);

        public static string GetTotalDuration(this BindableList<PlaylistItem> playlist) =>
            playlist.Select(p => p.Beatmap.Value.Length).Sum().Milliseconds().Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, precision: 2);
    }
}
