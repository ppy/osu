// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class HalfTimeModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.HalfTimeMod";

        /// <summary>
        /// "Less zoom..."
        /// </summary>
        public static LocalisableString HalfTimeDescription => new TranslatableString(getKey(@"half_time_description"), "Less zoom...");

        /// <summary>
        /// "Speed decrease"
        /// </summary>
        public static LocalisableString SpeedChange => new TranslatableString(getKey(@"speed_change"), "Speed decrease");

        /// <summary>
        /// "The actual decrease to apply"
        /// </summary>
        public static LocalisableString SpeedChangeDescription => new TranslatableString(getKey(@"speed_change_description"), "The actual decrease to apply");

        /// <summary>
        /// "Whoaaaaa..."
        /// </summary>
        public static LocalisableString DaycoreDescription => new TranslatableString(getKey(@"daycore_description"), "Whoaaaaa...");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
