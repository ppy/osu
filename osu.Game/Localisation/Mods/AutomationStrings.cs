// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class AutomationStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.Automation";

        /// <summary>
        /// "Whatch a perfect automated play through the song."
        /// </summary>
        public static LocalisableString AutoplayDescription => new TranslatableString(getKey(@"autoplay_desctiption"), "Whatch a perfect automated play through the song.");

        /// <summary>
        /// "Watch the video without visual distractions."
        /// </summary>
        public static LocalisableString CinemaDescription => new TranslatableString(getKey(@"cinema_description"), "Watch the video without visual distractions.");

        /// <summary>
        /// "You don't need to click. Give your clicking/tapping fingers a break from the heat of things."
        /// </summary>
        public static LocalisableString OsuRelaxDescription => new TranslatableString(getKey(@"osu_relax_description"), "You don't need to click. Give your clicking/tapping fingers a break from the heat of things.");

        /// <summary>
        /// "No ninja-like spinners, demanding drumrolls or unexpected katu's."
        /// </summary>
        public static LocalisableString TaikoRelaxDescription => new TranslatableString(getKey(@"taiko_relax_description"), "No ninja-like spinners, demanding drumrolls or unexpected katu's.");

        /// <summary>
        /// "Use the mouse to control the catcher."
        /// </summary>
        public static LocalisableString CatchRelaxDescription => new TranslatableString(getKey(@"catch_relax_description"), "Use the mouse to control the catcher.");

        /// <summary>
        /// "Automatic cursor movement - just follow the rhythm."
        /// </summary>
        public static LocalisableString OsuAutopilotDescription => new TranslatableString(getKey(@"osu_autopilot_description"), "Automatic cursor movement - just follow the rhythm.");

        /// <summary>
        /// "Spinners will be automatically completed."
        /// </summary>
        public static LocalisableString OsuSpunOutDescription => new TranslatableString(getKey(@"osu_spun_out_description"), "Spinners will be automatically completed.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
