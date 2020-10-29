// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchExtra
    {
        [Description("有视频")]
        Video,

        [Description("有故事版")]
        Storyboard
    }
}
