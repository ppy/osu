// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class SetupStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.Setup";

        /// <summary>
        /// "Not found"
        /// </summary>
        public static LocalisableString NotFound => new TranslatableString(getKey(@"not_found"), @"Not found");

        /// <summary>
        /// "Current IPC source"
        /// </summary>
        public static LocalisableString CurrentIPCSource => new TranslatableString(getKey(@"current_ipc_source"), @"Current IPC source");

        /// <summary>
        /// "Change source"
        /// </summary>
        public static LocalisableString ChangeIPCSource => new TranslatableString(getKey(@"change_ipc_source"), @"Change source");

        /// <summary>
        /// "The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation."
        /// </summary>
        public static LocalisableString IPCSourceDescription => new TranslatableString(getKey(@"ipc_source_description"),
            @"The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation.");

        /// <summary>
        /// "Current user"
        /// </summary>
        public static LocalisableString CurrentUser => new TranslatableString(getKey(@"current_user"), @"Current user");

        /// <summary>
        /// "Change sign-in"
        /// </summary>
        public static LocalisableString ChangeSignin => new TranslatableString(getKey(@"change_sign_in"), @"Change sign-in");

        /// <summary>
        /// "In order to access the API and display metadata, signing in is required."
        /// </summary>
        public static LocalisableString CurrentUserDescription => new TranslatableString(getKey(@"current_user_description"),
            @"In order to access the API and display metadata, signing in is required.");

        /// <summary>
        /// "Current tournament"
        /// </summary>
        public static LocalisableString CurrentTournament => new TranslatableString(getKey(@"current_tournament"), @"Current tournament");

        /// <summary>
        /// "Changes the background videos and bracket to match the selected tournament. This requires a restart to apply changes."
        /// </summary>
        public static LocalisableString CurrentTournamentDescription => new TranslatableString(getKey(@"current_tournament_description"),
            @"Changes the background videos and bracket to match the selected tournament. This requires a restart to apply changes.");

        /// <summary>
        /// "Stream area resolution"
        /// </summary>
        public static LocalisableString Resolution => new TranslatableString(getKey(@"resolution"), @"Stream area resolution");

        /// <summary>
        /// "Set height"
        /// </summary>
        public static LocalisableString SetResolution => new TranslatableString(getKey(@"set_resolution"), @"Set height");

        /// <summary>
        /// "Ruleset"
        /// </summary>
        public static LocalisableString Ruleset => new TranslatableString(getKey(@"ruleset"), @"Ruleset");

        /// <summary>
        /// "Decides what stats are displayed and which ranks are retrieved for players. This requires a restart to reload data for an existing bracket."
        /// </summary>
        public static LocalisableString RulesetDescription => new TranslatableString(getKey(@"ruleset_description"),
            @"Decides what stats are displayed and which ranks are retrieved for players. This requires a restart to reload data for an existing bracket.");

        /// <summary>
        /// "Display team seeds"
        /// </summary>
        public static LocalisableString DisplaySeeds => new TranslatableString(getKey(@"display_seeds"), @"Display team seeds");

        /// <summary>
        /// "Team seeds will display alongside each team at the top in gameplay/map pool screens."
        /// </summary>
        public static LocalisableString DisplaySeedsDescription => new TranslatableString(getKey(@"display_seeds_description"),
            @"Team seeds will display alongside each team at the top in gameplay/map pool screens.");

        /// <summary>
        /// "Auto advance screens"
        /// </summary>
        public static LocalisableString AutoAdvanceScreens => new TranslatableString(getKey(@"auto_advance_screens"), @"Auto advance screens");

        /// <summary>
        /// "Screens will progress automatically from gameplay -> results -> map pool"
        /// </summary>
        public static LocalisableString AutoAdvanceScreensDescription => new TranslatableString(getKey(@"auto_advance_screens_description"),
            @"Screens will progress automatically from gameplay -> results -> map pool");

        /// <summary>
        /// "Open folder"
        /// </summary>
        public static LocalisableString OpenFolder => new TranslatableString(getKey(@"open_folder"), @"Open folder");

        /// <summary>
        /// "Close osu!"
        /// </summary>
        public static LocalisableString CloseOsu => new TranslatableString(getKey(@"close_osu"), @"Close osu!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
