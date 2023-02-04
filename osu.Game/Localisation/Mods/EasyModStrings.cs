// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class EasyModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.EasyMod";

        /// <summary>
        /// "Larger circles, more forgiving HP drain, less accuracy required, and three lives!"
        /// </summary>
        public static LocalisableString OsuDescription =>
            new TranslatableString(getKey(@"osu_desctiption"), "Larger circles, more forgiving HP drain, less accuracy required, and three lives!");

        /// <summary>
        /// "Beats move slower, and less accuracy required!"
        /// </summary>
        public static LocalisableString TaikoDescription => new TranslatableString(getKey(@"taiko_description"), "Beats move slower, and less accuracy required!");

        /// <summary>
        /// "More forgiving HP drain, less accuracy, required, and three lives!"
        /// </summary>
        public static LocalisableString ManiaDescription => new TranslatableString(getKey(@"mania_description"), "More forgiving HP drain, less accuracy, required, and three lives!");

        /// <summary>
        /// "Larger fruits, more forgiving HP drain, less accuracy required, and three lives!"
        /// </summary>
        public static LocalisableString CatchDescription =>
            new TranslatableString(getKey(@"catch_description"), "Larger fruits, more forgiving HP drain, less accuracy required, and three lives!");

        /// <summary>
        /// "Extra lives"
        /// </summary>
        public static LocalisableString Retries => new TranslatableString(getKey(@"retries"), "Extra lives");

        /// <summary>
        /// "Number of extra lives"
        /// </summary>
        public static LocalisableString RetriesDescription => new TranslatableString(getKey(@"retries_description"), "Number of extra lives");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
