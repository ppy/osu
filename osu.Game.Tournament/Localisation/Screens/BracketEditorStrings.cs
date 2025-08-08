// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class BracketEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screens.BracketEditor";

        /// <summary>
        /// "Losers {0}"
        /// </summary>
        public static LocalisableString LosersRound(string name) => new TranslatableString(getKey(@"losers_round"), @"Losers {0}", name);

        /// <summary>
        /// "Right click to place and link matches"
        /// </summary>
        public static LocalisableString EmptyBracketPrompt => new TranslatableString(getKey(@"empty_bracket_prompt"), @"Right click to place and link matches");

        /// <summary>
        /// "Create new match"
        /// </summary>
        public static LocalisableString CreateNewMatch => new TranslatableString(getKey(@"create_new_match"), @"Create new match");

        /// <summary>
        /// "Reset teams"
        /// </summary>
        public static LocalisableString ResetTeams => new TranslatableString(getKey(@"reset_teams"), @"Reset teams");

        /// <summary>
        /// "Losers Bracket"
        /// </summary>
        public static LocalisableString LosersBracket => new TranslatableString(getKey(@"losers_bracket"), @"Losers Bracket");

        /// <summary>
        /// "Set as current"
        /// </summary>
        public static LocalisableString SetAsCurrent => new TranslatableString(getKey(@"set_as_current"), @"Set as current");

        /// <summary>
        /// "Join with"
        /// </summary>
        public static LocalisableString JoinWith => new TranslatableString(getKey(@"join_with"), @"Join with");

        /// <summary>
        /// "Join with (loser)"
        /// </summary>
        public static LocalisableString JoinWithLoser => new TranslatableString(getKey(@"join_with_loser"), @"Join with (loser)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

