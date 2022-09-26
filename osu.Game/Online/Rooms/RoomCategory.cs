// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;

namespace osu.Game.Online.Rooms
{
    public enum RoomCategory
    {
        // used for osu-web deserialization so names shouldn't be changed.
        [Description("正常")]
        Normal,

        [Description("聚光灯")]
        Spotlight,

        [Description("精选艺术家")]
        FeaturedArtist,
    }
}
