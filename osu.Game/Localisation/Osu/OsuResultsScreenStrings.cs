// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Osu
{
    public static class OsuResultsScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.OsuResultsScreen";

        /// <summary>
        /// "Accuracy Heatmap"
        /// </summary>
        public static LocalisableString AccuracyHeatmapPanelHeader => new TranslatableString(getKey(@"accuracy_heatmap_panel_header"), @"Accuracy Heatmap");

        /// <summary>
        /// "Undershoot"
        /// </summary>
        public static LocalisableString AccuracyHeatmapUndershoot => new TranslatableString(getKey(@"accuracy_heatmap_undershoot"), @"Undershoot");

        /// <summary>
        /// "Overshoot"
        /// </summary>
        public static LocalisableString AccuracyHeatmapOvershoot => new TranslatableString(getKey(@"accuracy_heatmap_overshoot"), @"Overshoot");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
