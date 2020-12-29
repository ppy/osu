// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using Humanizer.Localisation;
using osu.Framework.Bindables;

namespace osu.Game.Online.Rooms
{
    public static class PlaylistExtensions
    {
        public static string GetTotalDuration(this BindableList<PlaylistItem> playlist) =>
            playlist.Select(p => p.Beatmap.Value.Length).Sum().Milliseconds().Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, precision: 2);
    }
}
