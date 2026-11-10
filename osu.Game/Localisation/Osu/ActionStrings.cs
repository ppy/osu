// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Osu
{
    public static class ActionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.Action";

        /// <summary>
        /// "Left button"
        /// </summary>
        public static LocalisableString LeftButton => new TranslatableString(getKey(@"left_button"), @"Left button");

        /// <summary>
        /// "Right button"
        /// </summary>
        public static LocalisableString RightButton => new TranslatableString(getKey(@"right_button"), @"Right button");

        /// <summary>
        /// "Smoke"
        /// </summary>
        public static LocalisableString Smoke => new TranslatableString(getKey(@"smoke"), @"Smoke");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
