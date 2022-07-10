// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public enum RoomStatusFilter
    {
        [Description("screen.multi.lounge.components.roomStatusFilter.open")]
        Open,

        [Description("screen.multi.lounge.components.roomStatusFilter.ended")]
        Ended,

        [Description("screen.multi.lounge.components.roomStatusFilter.participated")]
        Participated,

        [Description("screen.multi.lounge.components.roomStatusFilter.owned")]
        Owned,
    }
}
