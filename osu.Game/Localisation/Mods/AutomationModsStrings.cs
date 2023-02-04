// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class AutomationModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.AutomationMods";

        /// <summary>
        /// "Automatic cursor movement - just follow the rhythm."
        /// </summary>
        public static LocalisableString AutopilotDescription => new TranslatableString(getKey(@"autopilot_description"), "Automatic cursor movement - just follow the rhythm.");

        /// <summary>
        /// "Watch a perfect automated play through the song."
        /// </summary>
        public static LocalisableString AutoplayDescription => new TranslatableString(getKey(@"autoplay_description"), "Watch a perfect automated play through the song.");

        private static string getKey(string key) => $"{prefix}:{key}";

        /// <summary>
        /// "Watch the video without visual distractions."
        /// </summary>
        public static LocalisableString CinemaDescription => new TranslatableString(getKey(@"cinema_description"), "Watch the video without visual distractions.");
    }
}
