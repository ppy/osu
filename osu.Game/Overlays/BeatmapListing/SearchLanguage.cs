// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Utils;

namespace osu.Game.Overlays.BeatmapListing
{
    [HasOrderedElements]
    public enum SearchLanguage
    {
        [Order(0)]
        Any,

        [Order(11)]
        Other,

        [Order(1)]
        English,

        [Order(6)]
        Japanese,

        [Order(2)]
        Chinese,

        [Order(10)]
        Instrumental,

        [Order(7)]
        Korean,

        [Order(3)]
        French,

        [Order(4)]
        German,

        [Order(9)]
        Swedish,

        [Order(8)]
        Spanish,

        [Order(5)]
        Italian
    }
}
