// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModPresetColumnStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModPresetColumn";

        /// <summary>
        /// "Personal Presets"
        /// </summary>
        public static LocalisableString PersonalPresets => new TranslatableString(getKey(@"personal_presets"), @"Personal Presets");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}