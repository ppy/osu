// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGeneral
    {
        [Description("Recommended difficulty")]
        Recommended,

        [Description("Include converted beatmaps")]
        Converts,

        [Description("Subscribed mappers")]
        Follows
    }
}
