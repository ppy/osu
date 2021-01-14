// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public enum RoomStatusFilter
    {
        Open,

        [Description("Recently Ended")]
        Ended,
        Participated,
        Owned,
    }
}
