// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class IncompatibilityDisplayingTooltipStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.IncompatibilityDisplayingTooltip";

        /// <summary>
        /// "Incompatible with:"
        /// </summary>
        public static LocalisableString IncompatibleWith => new TranslatableString(getKey(@"incompatible_with"), @"Incompatible with:");

        /// <summary>
        /// "Compatible with all mods"
        /// </summary>
        public static LocalisableString CompatibleWithAllMods => new TranslatableString(getKey(@"compatible_with_all_mods"), @"Compatible with all mods");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}