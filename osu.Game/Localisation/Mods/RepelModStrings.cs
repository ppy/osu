// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class RepelModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.RepelMod";

        /// <summary>
        /// "Hit objects run away!"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Hit objects run away!");

        /// <summary>
        /// "Repulsion strength"
        /// </summary>
        public static LocalisableString RepulsionStrength => new TranslatableString(getKey(@"repulsion_strength"), "Repulsion strength");

        /// <summary>
        /// "How strong the repulsion is."
        /// </summary>
        public static LocalisableString RepulsionStrengthDescription => new TranslatableString(getKey(@"repulsion_strength_description"), "How strong the repulsion is.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
