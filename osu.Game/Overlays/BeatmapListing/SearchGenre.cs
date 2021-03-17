// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGenre
    {
        Any = 0,
        Unspecified = 1,

        [Description("Video Game")]
        VideoGame = 2,
        Anime = 3,
        Rock = 4,
        Pop = 5,
        Other = 6,
        Novelty = 7,

        [Description("Hip Hop")]
        HipHop = 9,
        Electronic = 10,
        Metal = 11,
        Classical = 12,
        Folk = 13,
        Jazz = 14
    }
}
