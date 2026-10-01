// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Configuration;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Users;

namespace osu.Game.Extensions
{
    /// <summary>
    /// Conversion utilities for the <see cref="Language"/> enum.
    /// </summary>
    public static class LanguageExtensions
    {
        /// <summary>
        /// Returns the culture code of the <see cref="CultureInfo"/> that corresponds to the supplied <paramref name="language"/>.
        /// </summary>
        /// <remarks>
        /// This is required as enum member names are not allowed to contain hyphens.
        /// </remarks>
        public static string ToCultureCode(this Language language)
        {
            if (language == Language.zh_hant)
                return @"zh-tw";

            return language.ToString().Replace("_", "-");
        }

        /// <summary>
        /// Attempts to parse the supplied <paramref name="cultureCode"/> to a <see cref="Language"/> value.
        /// </summary>
        /// <param name="cultureCode">The code of the culture to parse.</param>
        /// <param name="language">The parsed <see cref="Language"/>. Valid only if the return value of the method is <see langword="true" />.</param>
        /// <returns>Whether the parsing succeeded.</returns>
        public static bool TryParseCultureCode(string cultureCode, out Language language)
        {
            if (cultureCode == @"zh-tw")
            {
                language = Language.zh_hant;
                return true;
            }

            return Enum.TryParse(cultureCode.Replace("-", "_"), out language);
        }

        /// <summary>
        /// Parses the <see cref="Language"/> that is specified in <paramref name="frameworkLocale"/>,
        /// or if that is not valid, the language of the current <see cref="ResourceManagerLocalisationStore"/> as exposed by <paramref name="localisationParameters"/>.
        /// </summary>
        /// <param name="frameworkLocale">The current <see cref="FrameworkSetting.Locale"/>.</param>
        /// <param name="localisationParameters">The current <see cref="LocalisationParameters"/> of the <see cref="LocalisationManager"/>.</param>
        /// <returns>The parsed language.</returns>
        public static Language GetLanguageFor(string frameworkLocale, LocalisationParameters localisationParameters)
        {
            // the usual case when the user has changed the language
            if (TryParseCultureCode(frameworkLocale, out var language))
                return language;

            if (localisationParameters.Store != null)
            {
                // startup case, locale not explicitly set, or the set language was removed in an update
                if (TryParseCultureCode(localisationParameters.Store.EffectiveCulture.Name, out language))
                    return language;
            }

            return Language.en;
        }

        /// <summary>
        /// Returns the <see cref="CountryCode"/> that corresponds to the supplied <paramref name="language"/>.
        /// </summary>
        public static CountryCode ToCountryCode(this Language language)
        {
            switch (language)
            {
                case Language.en: return CountryCode.US;

                case Language.be: return CountryCode.BY;

                case Language.bg: return CountryCode.BG;

                case Language.ca: return CountryCode.ES;

                case Language.cs: return CountryCode.CZ;

                case Language.da: return CountryCode.DK;

                case Language.de: return CountryCode.DE;

                case Language.el: return CountryCode.GR;

                case Language.es: return CountryCode.ES;

                case Language.fi: return CountryCode.FI;

                case Language.fr: return CountryCode.FR;

                case Language.hr_hr: return CountryCode.HR;

                case Language.hu: return CountryCode.HU;

                case Language.id: return CountryCode.ID;

                case Language.it: return CountryCode.IT;

                case Language.ja: return CountryCode.JP;

                case Language.ko: return CountryCode.KR;

                case Language.lt: return CountryCode.LT;

                case Language.lv_lv: return CountryCode.LV;

                case Language.ms_my: return CountryCode.MY;

                case Language.nl: return CountryCode.NL;

                case Language.no: return CountryCode.NO;

                case Language.pl: return CountryCode.PL;

                case Language.pt: return CountryCode.PT;

                case Language.pt_br: return CountryCode.BR;

                case Language.ro: return CountryCode.RO;

                case Language.ru: return CountryCode.RU;

                case Language.sk: return CountryCode.SK;

                case Language.sl: return CountryCode.SI;

                case Language.sr: return CountryCode.RS;

                case Language.sv: return CountryCode.SE;

                case Language.th: return CountryCode.TH;

                case Language.tr: return CountryCode.TR;

                case Language.uk: return CountryCode.UA;

                case Language.vi: return CountryCode.VN;

                case Language.zh: return CountryCode.CN;

                case Language.zh_hant: return CountryCode.TW;

                default: return CountryCode.Unknown;
            }
        }
    }
}
