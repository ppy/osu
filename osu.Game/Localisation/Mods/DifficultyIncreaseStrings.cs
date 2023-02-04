// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class DifficultyIncreaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.DifficultyIncrease";

        /// <summary>
        /// "Everything just got a bit harder..."
        /// </summary>
        public static LocalisableString HardRockDescription => new TranslatableString(getKey(@"hard_rock_description"), "Everything just got a bit harder...");

        /// <summary>
        /// "Play with blinds on your screen."
        /// </summary>
        public static LocalisableString OsuBlindsDescription => new TranslatableString(getKey(@"osu_blinds_description"), "Play with blinds on your screen.");

        /// <summary>
        /// "Once you start a slider, follow precisely or get a miss."
        /// </summary>
        public static LocalisableString OsuStrictTrackingDescription => new TranslatableString(getKey(@"osu_strict_tracking_description"), "Once you start a slider, follow precisely or get a miss.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
