// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class ScaleTweenModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.ScaleTweenMods";

        /// <summary>
        /// "Starting Size"
        /// </summary>
        public static LocalisableString StartScale => new TranslatableString(getKey(@"osu_object_scale_tween_start_scale"), "Starting Size");

        /// <summary>
        /// "The initial size multiplier applied to all object."
        /// </summary>
        public static LocalisableString StartScaleDescription =>
            new TranslatableString(getKey(@"osu_object_scale_tween_start_scale_description"), "The initial size multiplier applied to all object.");

        /// <summary>
        /// "Hit them at the right size!"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"osu_object_scale_tween_description"), "Hit them at the right size!");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
