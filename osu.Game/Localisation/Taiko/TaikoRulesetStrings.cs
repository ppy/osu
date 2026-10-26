// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Taiko
{
    public static class TaikoRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Taiko.TaikoRuleset";

        /// <summary>
        /// "Affects timing requirements for hits and mash rate requirements for swells."
        /// </summary>
        public static LocalisableString AccuracyDescription => new TranslatableString(getKey(@"accuracy_description"), @"Affects timing requirements for hits and mash rate requirements for swells.");

        /// <summary>
        /// "Hits per second required to clear swells"
        /// </summary>
        public static LocalisableString HitsPerSecondRequiredToClearSwells => new TranslatableString(getKey(@"hits_per_second_required_to_clear_swells"), @"Hits per second required to clear swells");

        /// <summary>
        /// "Multiplier applied to the baseline scroll speed of the playfield."
        /// </summary>
        public static LocalisableString ScrollSpeedDescription => new TranslatableString(getKey(@"scroll_speed_description"), @"Multiplier applied to the baseline scroll speed of the playfield.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
