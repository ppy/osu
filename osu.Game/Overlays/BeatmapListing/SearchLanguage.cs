// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [HasOrderedElements]
    public enum SearchLanguage
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageAny))]
        [Order(0)]
        Any,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageUnspecified))]
        [Order(14)]
        Unspecified,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageEnglish))]
        [Order(1)]
        English,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageJapanese))]
        [Order(6)]
        Japanese,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageChinese))]
        [Order(2)]
        Chinese,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageInstrumental))]
        [Order(12)]
        Instrumental,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageKorean))]
        [Order(7)]
        Korean,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageFrench))]
        [Order(3)]
        French,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageGerman))]
        [Order(4)]
        German,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageSwedish))]
        [Order(9)]
        Swedish,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageSpanish))]
        [Order(8)]
        Spanish,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageItalian))]
        [Order(5)]
        Italian,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageRussian))]
        [Order(10)]
        Russian,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguagePolish))]
        [Order(11)]
        Polish,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.LanguageOther))]
        [Order(13)]
        Other
    }
}
