// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsBeatmapAvailabilityTracker : OnlinePlayBeatmapAvailabilityTracker
    {
        public new Bindable<PlaylistItem?> PlaylistItem => base.PlaylistItem;
    }
}
