// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
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
            => language.ToString().Replace("_", "-");

        /// <summary>
        /// Attempts to parse the supplied <paramref name="cultureCode"/> to a <see cref="Language"/> value.
        /// </summary>
        /// <param name="cultureCode">The code of the culture to parse.</param>
        /// <param name="language">The parsed <see cref="Language"/>. Valid only if the return value of the method is <see langword="true" />.</param>
        /// <returns>Whether the parsing succeeded.</returns>
        public static bool TryParseCultureCode(string cultureCode, out Language language)
            => Enum.TryParse(cultureCode.Replace("-", "_"), out language);
    }
}
