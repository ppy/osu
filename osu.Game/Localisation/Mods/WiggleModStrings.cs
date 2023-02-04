// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class WiggleModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.WiggleMod";

        /// <summary>
        /// "They just won't stay still..."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "They just won't stay still...");

        /// <summary>
        /// "Strength"
        /// </summary>
        public static LocalisableString Strength => new TranslatableString(getKey(@"strength"), "Strength");

        /// <summary>
        /// "Multiplier applied to the wiggling strength."
        /// </summary>
        public static LocalisableString StrengthDescription => new TranslatableString(getKey(@"strength_description"), "Multiplier applied to the wiggling strength.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
