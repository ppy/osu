// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class MirrorModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MirrorMod";

        /// <summary>
        /// "Flip objects on the chosen axes."
        /// </summary>
        public static LocalisableString OsuDescription => new TranslatableString(getKey(@"osu_description"), "Flip objects on the chosen axes.");

        /// <summary>
        /// "Mirrored axes"
        /// </summary>
        public static LocalisableString OsuReflection => new TranslatableString(getKey(@"osu_reflection"), "Mirrored axes");

        /// <summary>
        /// "Choose which axes objects are mirrored over."
        /// </summary>
        public static LocalisableString OsuReflectionDescription => new TranslatableString(getKey(@"osu_reflection_description"), "Choose which axes objects are mirrored over.");

        /// <summary>
        /// "Horizontal"
        /// </summary>
        public static LocalisableString OsuMirrorTypeHorizontal => new TranslatableString(getKey(@"osu_mirror_type_horizontal"), "Horizontal");

        /// <summary>
        /// "Vertical"
        /// </summary>
        public static LocalisableString OsuMirrorTypeVertical => new TranslatableString(getKey(@"osu_mirror_type_vertical"), "Vertical");

        /// <summary>
        /// "Both"
        /// </summary>
        public static LocalisableString OsuMirrorTypeBoth => new TranslatableString(getKey(@"osu_mirror_type_both"), "Both");

        /// <summary>
        /// "Fruits are flipped horizontally."
        /// </summary>
        public static LocalisableString CatchDescription => new TranslatableString(getKey(@"catch_description"), "Fruits are flipped horizontally.");

        /// <summary>
        /// "Notes are flipped horizontally."
        /// </summary>
        public static LocalisableString ManiaDescription => new TranslatableString(getKey(@"mania_description"), "Notes are flipped horizontally.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
