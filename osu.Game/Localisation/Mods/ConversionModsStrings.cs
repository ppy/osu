// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class ConversionModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.ConversionMods";

        /// <summary>
        /// "Double the stages, double the fun!"
        /// </summary>
        public static LocalisableString ManiaDualStagesDescription => new TranslatableString(getKey(@"mania_dual_stages_description"), "Double the stages, double the fun!");

        /// <summary>
        /// "Hold the keys. To the beat."
        /// </summary>
        public static LocalisableString ManiaInvertDescription => new TranslatableString(getKey(@"mania_invert_description"), "Hold the keys. To the beat.");

        /// <summary>
        /// "No more tricky speed changes!"
        /// </summary>
        public static LocalisableString ManiaConstantSpeedDescription => new TranslatableString(getKey(@"mania_constant_speed_description"), "No more tricky speed changes!");

        /// <summary>
        /// "Replaces all hold notes with normal notes."
        /// </summary>
        public static LocalisableString ManiaHoldOffDescription => new TranslatableString(getKey(@"mania_old_off_description"), "Replaces all hold notes with normal notes.");

        /// <summary>
        /// "Don't use the same key twice in a row!"
        /// </summary>
        public static LocalisableString OsuAlternateDescription => new TranslatableString(getKey(@"osu_alternate_description"), "Don't use the same key twice in a row!");

        /// <summary>
        /// "You must only use one key!"
        /// </summary>
        public static LocalisableString OsuSingleTapDescription => new TranslatableString(getKey(@"osu_singletap_description"), "You must only use one key!");

        /// <summary>
        /// "Dons become kats, kats become dons"
        /// </summary>
        public static LocalisableString TaikoSwapDescription => new TranslatableString(getKey(@"taiko_swap_description"), "Dons become kats, kats become dons");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
