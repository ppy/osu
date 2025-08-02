// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class BeatmapLeaderboardWedgeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapLeaderboardWedge";

        /// <summary>
        /// "Scope"
        /// </summary>
        public static LocalisableString Scope => new TranslatableString(getKey(@"scope"), @"Scope");

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString Local => new TranslatableString(getKey(@"local"), @"Local");

        /// <summary>
        /// "Global"
        /// </summary>
        public static LocalisableString Global => new TranslatableString(getKey(@"global"), @"Global");

        /// <summary>
        /// "Country"
        /// </summary>
        public static LocalisableString Country => new TranslatableString(getKey(@"country"), @"Country");

        /// <summary>
        /// "Friend"
        /// </summary>
        public static LocalisableString Friend => new TranslatableString(getKey(@"friend"), @"Friend");

        /// <summary>
        /// "Team"
        /// </summary>
        public static LocalisableString Team => new TranslatableString(getKey(@"team"), @"Team");

        /// <summary>
        /// "Sort"
        /// </summary>
        public static LocalisableString Sort => new TranslatableString(getKey(@"sort"), @"Sort");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString Score => new TranslatableString(getKey(@"score"), @"Score");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString Accuracy => new TranslatableString(getKey(@"accuracy"), @"Accuracy");

        /// <summary>
        /// "Max Combo"
        /// </summary>
        public static LocalisableString MaxCombo => new TranslatableString(getKey(@"max_combo"), @"Max Combo");

        /// <summary>
        /// "Misses"
        /// </summary>
        public static LocalisableString Misses => new TranslatableString(getKey(@"misses"), @"Misses");

        /// <summary>
        /// "Date"
        /// </summary>
        public static LocalisableString Date => new TranslatableString(getKey(@"date"), @"Date");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
