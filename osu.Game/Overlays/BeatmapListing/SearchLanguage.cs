// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchLanguageEnumLocalisationMapper))]
    [HasOrderedElements]
    public enum SearchLanguage
    {
        [Description("任意")]
        [Order(0)]
        Any,

        [Description("未指定")]
        [Order(14)]
        Unspecified,

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
        [Order(12)]
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
        Italian,

        [Description("俄语")]
        [Order(10)]
        Russian,

        [Description("波兰语")]
        [Order(11)]
        Polish,

        [Description("其他")]
        [Order(13)]
        Other
    }

    public class SearchLanguageEnumLocalisationMapper : EnumLocalisationMapper<SearchLanguage>
    {
        public override LocalisableString Map(SearchLanguage value)
        {
            switch (value)
            {
                case SearchLanguage.Any:
                    return BeatmapsStrings.LanguageAny;

                case SearchLanguage.Unspecified:
                    return BeatmapsStrings.LanguageUnspecified;

                case SearchLanguage.English:
                    return BeatmapsStrings.LanguageEnglish;

                case SearchLanguage.Japanese:
                    return BeatmapsStrings.LanguageJapanese;

                case SearchLanguage.Chinese:
                    return BeatmapsStrings.LanguageChinese;

                case SearchLanguage.Instrumental:
                    return BeatmapsStrings.LanguageInstrumental;

                case SearchLanguage.Korean:
                    return BeatmapsStrings.LanguageKorean;

                case SearchLanguage.French:
                    return BeatmapsStrings.LanguageFrench;

                case SearchLanguage.German:
                    return BeatmapsStrings.LanguageGerman;

                case SearchLanguage.Swedish:
                    return BeatmapsStrings.LanguageSwedish;

                case SearchLanguage.Spanish:
                    return BeatmapsStrings.LanguageSpanish;

                case SearchLanguage.Italian:
                    return BeatmapsStrings.LanguageItalian;

                case SearchLanguage.Russian:
                    return BeatmapsStrings.LanguageRussian;

                case SearchLanguage.Polish:
                    return BeatmapsStrings.LanguagePolish;

                case SearchLanguage.Other:
                    return BeatmapsStrings.LanguageOther;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
