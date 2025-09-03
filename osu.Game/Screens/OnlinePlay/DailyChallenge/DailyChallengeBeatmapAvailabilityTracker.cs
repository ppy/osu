// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeBeatmapAvailabilityTracker : OnlinePlayBeatmapAvailabilityTracker
    {
        public DailyChallengeBeatmapAvailabilityTracker(PlaylistItem item)
        {
            PlaylistItem.Value = item;
        }
    }
}
