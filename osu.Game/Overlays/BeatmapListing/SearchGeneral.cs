// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGeneral
    {
        [Description("推荐难度")]
        Recommended,

        [Description("包括转谱")]
        Converts,

        [Description("已关注的谱师")]
        Follows
    }
}
