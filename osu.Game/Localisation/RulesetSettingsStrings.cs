// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RulesetSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RulesetSettings";

        /// <summary>
        /// "Rulesets"
        /// </summary>
        public static LocalisableString Rulesets => new TranslatableString(getKey(@"rulesets"), @"Rulesets");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString None => new TranslatableString(getKey(@"none"), @"None");

        /// <summary>
        /// "Corners"
        /// </summary>
        public static LocalisableString Corners => new TranslatableString(getKey(@"corners"), @"Corners");

        /// <summary>
        /// "Full"
        /// </summary>
        public static LocalisableString Full => new TranslatableString(getKey(@"full"), @"Full");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
