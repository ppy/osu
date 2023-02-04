// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class FlashlightModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.FlashlightMod";

        /// <summary>
        /// "Restricted view area."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), @"Restricted view area.");

        /// <summary>
        /// "Flashlight size"
        /// </summary>
        public static LocalisableString SizeMultiplier => new TranslatableString(getKey(@"size_multiplier"), @"Flashlight size");

        /// <summary>
        /// "Multiplier applied to the default flashlight size."
        /// </summary>
        public static LocalisableString SizeMultiplierDescription => new TranslatableString(getKey(@"size_multiplier_description"), @"Multiplier applied to the default flashlight size.");

        /// <summary>
        /// "Change size based on combo"
        /// </summary>
        public static LocalisableString ComboBasedSize => new TranslatableString(getKey(@"combo_based_size"), @"Change size based on combo");

        /// <summary>
        /// "Decrease the flashlight size as combo increases."
        /// </summary>
        public static LocalisableString ComboBasedSizeDescription => new TranslatableString(getKey(@"combo_based_size_description"), @"Decrease the flashlight size as combo increases.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
