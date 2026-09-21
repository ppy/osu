// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Osu
{
    public static class OsuRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.OsuRuleset";

        /// <summary>
        /// "Affects the size of hit circles and sliders."
        /// </summary>
        public static LocalisableString CircleSizeDescription => new TranslatableString(getKey(@"circle_size_description"), @"Affects the size of hit circles and sliders.");

        /// <summary>
        /// "Hit circle radius"
        /// </summary>
        public static LocalisableString HitCircleRadius => new TranslatableString(getKey(@"hit_circle_radius"), @"Hit circle radius");

        /// <summary>
        /// "Affects how early objects appear on screen relative to their hit time."
        /// </summary>
        public static LocalisableString ApproachRateDescription => new TranslatableString(getKey(@"approach_rate_description"), @"Affects how early objects appear on screen relative to their hit time.");

        /// <summary>
        /// "Approach time"
        /// </summary>
        public static LocalisableString ApproachTime => new TranslatableString(getKey(@"approach_time"), @"Approach time");

        /// <summary>
        /// "Affects timing requirements for hit circles and spin speed requirements for spinners."
        /// </summary>
        public static LocalisableString AccuracyDescription => new TranslatableString(getKey(@"accuracy_description"), @"Affects timing requirements for hit circles and spin speed requirements for spinners.");

        /// <summary>
        /// "RPM required to clear spinners"
        /// </summary>
        public static LocalisableString RpmRequiredToClearSpinners => new TranslatableString(getKey(@"rpm_required_to_clear_spinners"), @"RPM required to clear spinners");

        /// <summary>
        /// "RPM required to get full spinner bonus"
        /// </summary>
        public static LocalisableString RpmRequiredToGetFullSpinnerBonus => new TranslatableString(getKey(@"rpm_required_to_get_full_spinner_bonus"), @"RPM required to get full spinner bonus");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
