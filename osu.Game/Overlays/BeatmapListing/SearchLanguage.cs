// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
