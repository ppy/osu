// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MultiplayerResultsScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MultiplayerResultsScreen";

        /// <summary>
        /// "Team {0} wins!"
        /// </summary>
        public static LocalisableString TeamWins(string winner) => new TranslatableString(getKey(@"team_wins"), @"Team {0} wins!", winner);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}