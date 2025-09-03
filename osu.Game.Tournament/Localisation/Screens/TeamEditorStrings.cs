// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class TeamEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.TeamEditor";

        /// <summary>
        /// "Team Information"
        /// </summary>
        public static LocalisableString TeamInfoHeader => new TranslatableString(getKey(@"team_info_header"), @"Team Information");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString TeamName => new TranslatableString(getKey(@"team_name"), @"Name");

        /// <summary>
        /// "Acronym"
        /// </summary>
        public static LocalisableString TeamAcronym => new TranslatableString(getKey(@"team_acronym"), @"Acronym");

        /// <summary>
        /// "Flag"
        /// </summary>
        public static LocalisableString TeamFlag => new TranslatableString(getKey(@"team_flag"), @"Flag");

        /// <summary>
        /// "Seed"
        /// </summary>
        public static LocalisableString TeamSeed => new TranslatableString(getKey(@"team_seed"), @"Seed");

        /// <summary>
        /// "Last Year Placement"
        /// </summary>
        public static LocalisableString LastYearPlacement => new TranslatableString(getKey(@"last_year_placement"), @"Last Year Placement");

        /// <summary>
        /// "Edit seeding results"
        /// </summary>
        public static LocalisableString EditSeedingResults => new TranslatableString(getKey(@"edit_seeding_results"), @"Edit seeding results");

        /// <summary>
        /// "Player List"
        /// </summary>
        public static LocalisableString PlayerListHeader => new TranslatableString(getKey(@"player_list_header"), @"Player List");

        /// <summary>
        /// "Add player"
        /// </summary>
        public static LocalisableString AddPlayer => new TranslatableString(getKey(@"add_player"), @"Add player");

        /// <summary>
        /// "Delete Team"
        /// </summary>
        public static LocalisableString DeleteTeam => new TranslatableString(getKey(@"delete_team"), @"Delete Team");

        /// <summary>
        /// "Add all countries"
        /// </summary>
        public static LocalisableString AddAllCountries => new TranslatableString(getKey(@"add_all_countries"), @"Add all countries");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

