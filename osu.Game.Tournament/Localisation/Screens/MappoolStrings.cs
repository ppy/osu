// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class MappoolStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.Mappool";

        /// <summary>
        /// "Current Mode"
        /// </summary>
        public static LocalisableString CurrentMode => new TranslatableString(getKey(@"current_mode"), @"Current Mode");

        /// <summary>
        /// "Red Ban"
        /// </summary>
        public static LocalisableString RedBan => new TranslatableString(getKey(@"red_ban"), "Red Ban");

        /// <summary>
        /// "Blue Ban"
        /// </summary>
        public static LocalisableString BlueBan => new TranslatableString(getKey(@"blue_ban"), @"Blue Ban");

        /// <summary>
        /// "Red Pick"
        /// </summary>
        public static LocalisableString RedPick => new TranslatableString(getKey(@"red_pick"), @"Red Pick");

        /// <summary>
        /// "Blue Pick"
        /// </summary>
        public static LocalisableString BluePick => new TranslatableString(getKey(@"blue_pick"), @"Blue Pick");

        /// <summary>
        /// "Split display by mods"
        /// </summary>
        public static LocalisableString SplitDisplayByMods => new TranslatableString(getKey(@"split_display_by_mods"), @"Split display by mods");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
