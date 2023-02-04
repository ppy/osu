// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class RandomModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.RandomMod";

        /// <summary>
        /// "Shuffle around the keys!"
        /// </summary>
        public static LocalisableString ManiaDescription => new TranslatableString(getKey(@"mania_description"), "Shuffle around the keys!");

        /// <summary>
        /// "It never gets boring!"
        /// </summary>
        public static LocalisableString OsuDescription => new TranslatableString(getKey(@"osu_description"), "It never gets boring!");

        /// <summary>
        /// "Angle sharpness"
        /// </summary>
        public static LocalisableString OsuAngleSharpness => new TranslatableString(getKey(@"osu_angle_sharpness"), "Angle sharpness");

        /// <summary>
        /// "How sharp angles should be"
        /// </summary>
        public static LocalisableString OsuAngleSharpnessDescription => new TranslatableString(getKey(@"osu_angle_sharpness_description"), "How sharp angles should be");

        /// <summary>
        /// "Shuffle around the colours!"
        /// </summary>
        public static LocalisableString TaikoDescription => new TranslatableString(getKey(@"taiko_description"), "Shuffle around the colours!");

        /// <summary>
        /// "Seed"
        /// </summary>
        public static LocalisableString Seed => new TranslatableString(getKey(@"seed"), "Seed");

        /// <summary>
        /// "Use a custom seed instead of a random one"
        /// </summary>
        public static LocalisableString SeedDescription => new TranslatableString(getKey(@"seed_description"), "Use a custom seed instead of a random one");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
