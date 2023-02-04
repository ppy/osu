// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class MagnetisedModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.MagnetisedMod";

        /// <summary>
        /// "No need to chase the circles - your cursor is a magnet!"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "No need to chase the circles - your cursor is a magnet!");

        /// <summary>
        /// "Attraction strength"
        /// </summary>
        public static LocalisableString AttractionStrength => new TranslatableString(getKey(@"attraction_strength"), "Attraction strength");

        /// <summary>
        /// "How strong the pull is."
        /// </summary>
        public static LocalisableString AttractionStrengthDescription => new TranslatableString(getKey(@"attraction_strength_description"), "How strong the pull is.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
