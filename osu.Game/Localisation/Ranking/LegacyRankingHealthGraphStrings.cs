// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Ranking
{
    public static class LegacyRankingHealthGraphStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Ranking.LegacyRankingHealthGraph";

        /// <summary>
        /// "Accuracy:
        /// Error: {0:0.00}ms - {1:0.00}ms avg
        /// Unstable Rate: {2:0.00}"
        /// </summary>
        public static LocalisableString TooltipText(double earlyAverage, double lateAverage, double unstableRate) => new TranslatableString(getKey(@"tooltip_text"), @"Accuracy:
Error: {0:0.00}ms - {1:0.00}ms avg
Unstable Rate: {2:0.00}", earlyAverage, lateAverage, unstableRate);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
