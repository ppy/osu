// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RankingStatisticsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RankingStatisticsStrings";

        /// <summary>
        /// "slider tick"
        /// </summary>
        public static LocalisableString OsuSliderTick => new TranslatableString(getKey(@"osu_slider_tick"), @"slider tick");

        /// <summary>
        /// "slider end"
        /// </summary>
        public static LocalisableString OsuSliderEnd => new TranslatableString(getKey(@"osu_slider_end"), @"slider end");

        /// <summary>
        /// "spinner spin"
        /// </summary>
        public static LocalisableString OsuSpinnerSpin => new TranslatableString(getKey(@"osu_spinner_spin"), @"spinner spin");

        /// <summary>
        /// "spinner bonus"
        /// </summary>
        public static LocalisableString OsuSpinnerBonus => new TranslatableString(getKey(@"osu_spinner_bonus"), @"spinner bonus");

        /// <summary>
        /// "Large droplet"
        /// </summary>
        public static LocalisableString CatchLargeDroplet => new TranslatableString(getKey(@"catch_large_droplet"), @"Large droplet");

        /// <summary>
        /// "Small droplet"
        /// </summary>
        public static LocalisableString CatchSmallDroplet => new TranslatableString(getKey(@"catch_small_droplet"), @"Small droplet");

        /// <summary>
        /// "Banana"
        /// </summary>
        public static LocalisableString CatchBanana => new TranslatableString(getKey(@"catch_banana"), @"Banana");

        /// <summary>
        /// "drum tick"
        /// </summary>
        public static LocalisableString TaikoDrumTick => new TranslatableString(getKey(@"taiko_drum_tick"), @"drum tick");

        /// <summary>
        /// "bonus"
        /// </summary>
        public static LocalisableString TaikoBonus => new TranslatableString(getKey(@"taiko_bonus"), @"bonus");

        /// <summary>
        /// "Performance Breakdown"
        /// </summary>
        public static LocalisableString PerformanceBreakdownTitle => new TranslatableString(getKey(@"performance_breakdown_title"), @"Performance Breakdown");

        /// <summary>
        /// "Timing Distribution"
        /// </summary>
        public static LocalisableString TimingDistributionTitle => new TranslatableString(getKey(@"timing_distribution_title"), @"Timing Distribution");

        /// <summary>
        /// "Accuracy Heatmap"
        /// </summary>
        public static LocalisableString AccuracyHeatmapTitle => new TranslatableString(getKey(@"accuracy_heatmap_title"), @"Accuracy Heatmap");

        /// <summary>
        /// "Statistics"
        /// </summary>
        public static LocalisableString StatisticsTitle => new TranslatableString(getKey(@"statistics_title"), @"Statistics");

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

        /// <summary>
        /// "Classic scoring mode is always used for this statistic."
        /// </summary>
        public static LocalisableString ClassicScoringAlwaysUsed => new TranslatableString(getKey(@"classic_scoring_always_used"), @"Classic scoring mode is always used for this statistic.");

        /// <summary>
        /// "More statistics available after watching a replay!"
        /// </summary>
        public static LocalisableString StatisticsAvailableAfterReplay => new TranslatableString(getKey(@"statistics_available_after_replay"), @"More statistics available after watching a replay!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
