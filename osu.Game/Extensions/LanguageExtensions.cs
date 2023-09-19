// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Configuration;
using osu.Framework.Localisation;
using osu.Game.Localisation;

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
    }
}
