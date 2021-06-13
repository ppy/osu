// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;

namespace osu.Game.Overlays.BeatmapListing
{
    [HasOrderedElements]
    public enum SearchLanguage
    {
        [Order(0)]
        Any,

        [Order(14)]
        Unspecified,

        [Order(1)]
        English,

        [Order(6)]
        Japanese,

        [Order(2)]
        Chinese,

        [Order(12)]
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
        Italian,

        [Order(10)]
        Russian,

        [Order(11)]
        Polish,

        [Order(13)]
        Other
    }
}
