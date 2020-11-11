// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchExtra
    {
        [Description("Has Video")]
        Video,

        [Description("Has Storyboard")]
        Storyboard
    }
}
