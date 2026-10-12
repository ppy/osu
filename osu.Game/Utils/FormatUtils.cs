// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Localisation;

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        public static double FloorToDecimalDigits(this double value, uint digits)
        {
            double base10 = Math.Pow(10, digits);
            return Math.Floor(value * base10) / base10;
        }

        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted.</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static LocalisableString FormatAccuracy(this double accuracy)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" percentage to the next whole number.
            // ie. a score which gets 89.99999% shouldn't ever show as 90%.
            // the reasoning for this is that cutoffs for grade increases are at whole numbers and displaying the required
            // percentile with a non-matching grade is confusing.
            return accuracy.FloorToDecimalDigits(4).ToLocalisableString("0.00%");
        }

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 10_000 ? 1 : 0);

        /// <summary>
        /// Formats the supplied star rating in a consistent, simplified way.
        /// </summary>
        /// <param name="starRating">The star rating to be formatted.</param>
        public static LocalisableString FormatStarRating(this double starRating)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" star rating to the next whole number.
            // i.e. a beatmap which has a star rating of 6.9999* should never show as 7.00*.
            // this matters for star rating medals which use hard cutoffs at whole numbers,
            // which then confuses users when they beat a 6.9999* beatmap but don't get the 7-star medal.
            return starRating.FloorToDecimalDigits(2).ToLocalisableString("0.00");
        }

        /// <summary>
        /// Finds the number of digits after the decimal.
        /// </summary>
        /// <param name="d">The value to find the number of decimal digits for.</param>
        /// <returns>The number decimal digits.</returns>
        public static int FindPrecision(decimal d)
        {
            int precision = 0;

            while (d != Math.Round(d))
            {
                d *= 10;
                precision++;
            }

            return precision;
        }

        /// <summary>
        /// Applies rounding to the given BPM value.
        /// </summary>
        /// <param name="baseBpm">The base BPM to round.</param>
        /// <param name="rate">Rate adjustment, if applicable.</param>
        public static int RoundBPM(double baseBpm, double rate = 1) => (int)Math.Round(baseBpm * rate);

        public static LocalisableString ToLocalisedMediumDate(this DateTimeOffset dateTime)
            => new LocalisableString(new MediumFormattedDate(dateTime));

        /// <summary>
        /// This class is supposed to provide date formatting roughly equivalent to
        /// <code>
        /// moment().format('ll');
        /// </code>
        /// which is used in several places on the website, and as such needs to be mirrored to the relevant game overlays reimplementing those places.
        /// </summary>
        private class MediumFormattedDate : ILocalisableStringData
        {
            public readonly DateTimeOffset Date;

            public MediumFormattedDate(DateTimeOffset date)
            {
                Date = date;
            }

            public bool Equals(ILocalisableStringData? other)
                => other is MediumFormattedDate date && Date.Equals(date.Date);

            // reference: individual language files in https://github.com/moment/moment/tree/18aba135ab927ffe7f868ee09276979bed6993a6/locale
            private static readonly Dictionary<Language, string> format_mapping = new Dictionary<Language, string>
            {
                [Language.en] = @"d MMM yyyy",
                [Language.be] = @"d MMM yyyy 'г.'",
                [Language.bg] = @"d MMM yyyy",
                [Language.ca] = @"d MMM yyyy",
                [Language.cs] = @"d. MMM yyyy",
                [Language.da] = @"d. MMM yyyy",
                [Language.de] = @"d. MMM yyyy",
                [Language.el] = @"d MMM yyyy",
                [Language.es] = @"d 'de' MMM 'de' yyyy",
                [Language.fi] = @"d. MMM yyyy",
                [Language.fr] = @"d MMM yyyy",
                [Language.hr_hr] = @"d. MMM yyyy",
                [Language.hu] = @"yyyy. MMM d.",
                [Language.id] = @"d MMM yyyy",
                [Language.it] = @"d MMM yyyy",
                [Language.ja] = @"yyyy年M月d日",
                [Language.ko] = @"yyyy년 MMMM d일",
                [Language.lt] = @"yyyy 'm.' MMM d 'd.'",
                [Language.lv_lv] = @"yyyy. 'gada' d. MMM",
                [Language.ms_my] = @"d MMM yyyy",
                [Language.nl] = @"d MMM yyyy",
                [Language.no] = @"d. MMM yyyy", // look under `nb` (Norsk Bokmål) and `nn` (Nynorsk) in momentjs source
                [Language.pl] = @"d MMM yyyy",
                [Language.pt] = @"d 'de' MMM 'de' yyyy",
                [Language.pt_br] = @"d 'de' MMM 'de' yyyy",
                [Language.ro] = @"d MMM yyyy",
                [Language.ru] = @"d MMM yyyy 'г.'",
                [Language.sk] = @"d. MMM yyyy",
                [Language.sl] = @"d. MMM yyyy",
                [Language.sr] = @"d. MMM yyyy.",
                [Language.sv] = @"d MMM yyyy",
                [Language.th] = @"d MMM yyyy",
                [Language.tr] = @"d MMM yyyy",
                [Language.uk] = @"d MMM yyyy 'р.'",
                [Language.vi] = @"d MMM yyyy",
                [Language.zh] = @"yyyy年M月d日",
                [Language.zh_hant] = @"yyyy年M月d日",
            };

            public string GetLocalised(LocalisationParameters parameters)
            {
                string? cultureCode = parameters.Store?.EffectiveCulture.Name.ToLowerInvariant();

                if (!string.IsNullOrEmpty(cultureCode)
                    && LanguageExtensions.TryParseCultureCode(cultureCode, out var language)
                    && format_mapping.TryGetValue(language, out string? format))
                {
                    return Date.ToString(format);
                }

                return Date.ToString(@"d MMM yyyy");
            }
        }
    }
}
