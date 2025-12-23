// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RankingStatisticsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RankingStatisticsStrings";

        /// <summary>
        /// "Average Hit Error"
        /// </summary>
        public static LocalisableString AverageHitErrorTitle => new TranslatableString(getKey(@"average_hit_error_title"), @"Average Hit Error");

        /// <summary>
        /// "Unstable Rate"
        /// </summary>
        public static LocalisableString UnstableRateTitle => new TranslatableString(getKey(@"unstable_rate_title"), @"Unstable Rate");

        /// <summary>
        /// "{0:N2} ms early"
        /// </summary>
        public static LocalisableString Early(double offset) => new TranslatableString(getKey(@"early"), @"{0:N2} ms early", offset);

        /// <summary>
        /// "{0:N2} ms late"
        /// </summary>
        public static LocalisableString Late(double offset) => new TranslatableString(getKey(@"late"), @"{0:N2} ms late", offset);

        /// <summary>
        /// "(not available)"
        /// </summary>
        public static LocalisableString NotAvailable => new TranslatableString(getKey(@"not_available"), @"(not available)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
