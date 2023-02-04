// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class RelaxModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.RelaxMod";

        /// <summary>
        /// "You don&#39;t need to click. Give your clicking/tapping fingers a break from the heat of things."
        /// </summary>
        public static LocalisableString OsuDescription => new TranslatableString(getKey(@"osu_description"), @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.");

        /// <summary>
        /// "No ninja-like spinners, demanding drum-rolls or unexpected katu's."
        /// </summary>
        public static LocalisableString TaikoDescription => new TranslatableString(getKey(@"taiko_description"), @"No ninja-like spinners, demanding drum-rolls or unexpected katu's.");

        /// <summary>
        /// "Use the mouse to control the catcher."
        /// </summary>
        public static LocalisableString CatchDescription => new TranslatableString(getKey(@"catch_description"), @"Use the mouse to control the catcher.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
