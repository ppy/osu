// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class BaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Base";

        /// <summary>
        /// "Populating user stats ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingUserStats(int current, int total) => new TranslatableString(getKey(@"populating_user_stats"),
            @"Populating user stats ({0} / {1})", current, total);

        /// <summary>
        /// "Populating round beatmaps ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingRoundBeatmaps(int current, int total) => new TranslatableString(getKey(@"populating_round_beatmaps"),
            @"Populating round beatmaps ({0} / {1})", current, total);

        /// <summary>
        /// "Populating seeding beatmaps ({0} / {1})"
        /// </summary>
        public static LocalisableString PopulatingSeedingBeatmaps(int current, int total) => new TranslatableString(getKey(@"populating_seeding_beatmaps"),
            @"Populating seeding beatmaps ({0} / {1})", current, total);

        /// <summary>
        /// "Your {0} file could not be parsed. Please check runtime.log for more details."
        /// </summary>
        public static LocalisableString BracketErrorWarning(string bracketName) => new TranslatableString(getKey(@"bracket_error_warning"),
            @"Your {0} file could not be parsed. Please check runtime.log for more details.", bracketName);

        /// <summary>
        /// "Please make the window wider"
        /// </summary>
        public static LocalisableString AspectRatioWarning => new TranslatableString(getKey(@"aspect_ratio_warning"),
            @"Please make the window wider");

        /// <summary>
        /// "Choose a match first from the brackets screen"
        /// </summary>
        public static LocalisableString NoMatchWarning => new TranslatableString(getKey(@"no_match_warning"),
            @"Choose a match first from the brackets screen");

        /// <summary>
        /// "Control Panel"
        /// </summary>
        public static LocalisableString ControlPanel => new TranslatableString(getKey(@"control_panel"), @"Control Panel");

        /// <summary>
        /// "Save Changes"
        /// </summary>
        public static LocalisableString SaveChanges => new TranslatableString(getKey(@"save_changes"), @"Save Changes");

        /// <summary>
        /// "Add New"
        /// </summary>
        public static LocalisableString AddNew => new TranslatableString(getKey(@"add_new"), @"Add New");

        /// <summary>
        /// "Clear All"
        /// </summary>
        public static LocalisableString Clear => new TranslatableString(getKey(@"clear"), @"Clear All");

        /// <summary>
        /// "Refresh"
        /// </summary>
        public static LocalisableString Refresh => new TranslatableString(getKey(@"refresh"), @"Refresh");

        /// <summary>
        /// "Reset"
        /// </summary>
        public static LocalisableString Reset => new TranslatableString(getKey(@"reset"), @"Reset");

        /// <summary>
        /// "Okay"
        /// </summary>
        public static LocalisableString Okay => new TranslatableString(getKey(@"okay"), @"Okay");

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"cancel"), @"Cancel");

        /// <summary>
        /// "Remove"
        /// </summary>
        public static LocalisableString Remove => new TranslatableString(getKey(@"remove"), @"Remove");

        /// <summary>
        /// "Round"
        /// </summary>
        public static LocalisableString Round => new TranslatableString(getKey(@"round"), @"Round");

        /// <summary>
        /// "Team Red"
        /// </summary>
        public static LocalisableString TeamRed => new TranslatableString(getKey(@"team_red"), @"Team Red");

        /// <summary>
        /// "Team Blue"
        /// </summary>
        public static LocalisableString TeamBlue => new TranslatableString(getKey(@"team_blue"), @"Team Blue");

        /// <summary>
        /// "Unknown Round"
        /// </summary>
        public static LocalisableString UnknownRound => new TranslatableString(getKey(@"unknown_round"), @"Unknown Round");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
