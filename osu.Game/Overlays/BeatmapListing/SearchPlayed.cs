// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchPlayed
    {
        [Description("任意")]
        Any,
        [Description("玩过")]
        Played,
        [Description("没玩过")]
        Unplayed
    }
}
