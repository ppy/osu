using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public static class DialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Dialog";

        /// <summary>
        /// "Delete team &quot;{0}&quot;?"
        /// </summary>
        public static LocalisableString DeleteTeamPrompt(string team) => new TranslatableString(getKey(@"delete_team_prompt"),
            @"Delete team ""{0}""?", team);

        /// <summary>
        /// "Delete unnamed team?"
        /// </summary>
        public static LocalisableString DeleteUnnamedTeamPrompt => new TranslatableString(getKey(@"delete_unnamed_team_prompt"), @"Delete unnamed team?");

        /// <summary>
        /// "Delete round &quot;{0}&quot;?"
        /// </summary>
        public static LocalisableString DeleteRoundPrompt(string round) => new TranslatableString(getKey(@"delete_round_prompt"),
            @"Delete round ""{0}""?", round);

        /// <summary>
        /// "Delete unnamed round?"
        /// </summary>
        public static LocalisableString DeleteUnnamedRoundPrompt => new TranslatableString(getKey(@"delete_unnamed_round_prompt"), @"Delete unnamed round?");

        /// <summary>
        /// "Reset teams?"
        /// </summary>
        public static LocalisableString ResetTeamsPrompt => new TranslatableString(getKey(@"reset_teams_prompt"), @"Reset teams?");

        /// <summary>
        /// "Clear all?"
        /// </summary>
        public static LocalisableString ClearAllPrompt => new TranslatableString(getKey(@"clear_all_prompt"), @"Clear all?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
