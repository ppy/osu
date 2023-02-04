// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class DoubleTimeModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.DoubleTime";

        /// <summary>
        /// "Zooooooooooom..."
        /// </summary>
        public static LocalisableString DoubleTimeDescription => new TranslatableString(getKey(@"double_time_description"), "Zooooooooooom...");

        /// <summary>
        /// "Speed increase"
        /// </summary>
        public static LocalisableString SpeedChange => new TranslatableString(getKey(@"speed_change"), "Speed increase");

        /// <summary>
        /// "The actual increase to apply"
        /// </summary>
        public static LocalisableString SpeedChangeDescription => new TranslatableString(getKey(@"speed_change_description"), "The actual increase to apply");

        /// <summary>
        /// "Uguuuuuuuu..."
        /// </summary>
        public static LocalisableString NightcoreDescription => new TranslatableString(getKey(@"nightcore_description"), "Uguuuuuuuu...");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
