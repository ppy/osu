// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class GameplayRankDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.GameplayRankDisplayStrings";

        /// <summary>
        /// "Rank display mode"
        /// </summary>
        public static LocalisableString RankDisplay => new TranslatableString(getKey(@"rank_display"), "Rank display mode");

        /// <summary>
        /// "Which rank mode should be displayed."
        /// </summary>
        public static LocalisableString RankDisplayDescription => new TranslatableString(getKey(@"rank_display_description"), "Which rank mode should be displayed.");

        /// <summary>
        /// "Standard"
        /// </summary>
        public static LocalisableString RankDisplayModeStandard => new TranslatableString(getKey(@"rank_display_mode_standard"), "Standard");

        /// <summary>
        /// "Maximum achievable"
        /// </summary>
        public static LocalisableString RankDisplayModeMax => new TranslatableString(getKey(@"rank_display_mode_max"), "Maximum achievable");

        /// <summary>
        /// "Minimum achievable"
        /// </summary>
        public static LocalisableString RankDisplayModeMin => new TranslatableString(getKey(@"rank_display_mode_min"), "Minimum achievable");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
