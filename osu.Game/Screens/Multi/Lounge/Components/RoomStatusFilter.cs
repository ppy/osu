// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public enum RoomStatusFilter
    {
        [Description("开放中的房间")]
        Open,

        [Description("最近关闭的房间")]
        Ended,

        [Description("参与过的房间")]
        Participated,

        [Description("拥有的房间")]
        Owned,
    }
}
