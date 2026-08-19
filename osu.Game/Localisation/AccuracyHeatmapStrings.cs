// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class AccuracyHeatmapStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.AccuracyHeatmap";

        /// <summary>
        /// "Accuracy Heatmap"
        /// </summary>
        public static LocalisableString AccuracyHeatmapHeader => new TranslatableString(getKey(@"accuracy_heatmap_header"), @"Accuracy Heatmap");

        /// <summary>
        /// "Overshoot"
        /// </summary>
        public static LocalisableString Overshoot => new TranslatableString(getKey(@"overshoot"), @"Overshoot");

        /// <summary>
        /// "Undershoot"
        /// </summary>
        public static LocalisableString Undershoot => new TranslatableString(getKey(@"undershoot"), @"Undershoot");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
