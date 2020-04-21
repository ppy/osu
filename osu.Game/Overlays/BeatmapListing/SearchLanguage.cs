// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Game.Utils;

namespace osu.Game.Overlays.BeatmapListing
{
    [HasOrderedElements]
    public enum SearchLanguage
    {
        [Description("任意")]
        [Order(0)]
        Any,

        [Description("其他")]
        [Order(11)]
        Other,

        [Description("英语")]
        [Order(1)]
        English,

        [Description("日语")]
        [Order(6)]
        Japanese,

        [Description("汉语")]
        [Order(2)]
        Chinese,

        [Description("乐器")]
        [Order(10)]
        Instrumental,

        [Description("韩语")]
        [Order(7)]
        Korean,

        [Description("法语")]
        [Order(3)]
        French,

        [Description("德语")]
        [Order(4)]
        German,

        [Description("瑞典语")]
        [Order(9)]
        Swedish,

        [Description("西班牙语")]
        [Order(8)]
        Spanish,

        [Description("意大利语")]
        [Order(5)]
        Italian
    }
}