// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public enum RoomModeFilter
    {
        [LocalisableDescription(typeof(OnlinePlayStrings), nameof(OnlinePlayStrings.RoomModeFilterOpen))]
        Open,

        [LocalisableDescription(typeof(OnlinePlayStrings), nameof(OnlinePlayStrings.RoomModeFilterEnded))]
        Ended,

        [LocalisableDescription(typeof(OnlinePlayStrings), nameof(OnlinePlayStrings.RoomModeFilterParticipated))]
        Participated,

        [LocalisableDescription(typeof(OnlinePlayStrings), nameof(OnlinePlayStrings.RoomModeFilterOwned))]
        Owned,
    }
}
