// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MultiplayerMatchStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MultiplayerMatchStrings";

        /// <summary>
        /// "Stop countdown"
        /// </summary>
        public static LocalisableString StopCountdown => new TranslatableString(getKey(@"stop_countdown"), @"Stop countdown");

        /// <summary>
        /// "Countdown settings"
        /// </summary>
        public static LocalisableString CountdownSettings => new TranslatableString(getKey(@"countdown_settings"), @"Countdown settings");

        /// <summary>
        /// "Start match in {0}"
        /// </summary>
        public static LocalisableString StartMatchWithCountdown(string humanReadableTime) => new TranslatableString(getKey(@"start_match_width_countdown"), @"Start match in {0}", humanReadableTime);

        /// <summary>
        /// "Choose the mods which all players should play with."
        /// </summary>
        public static LocalisableString RequiredModsButtonTooltip => new TranslatableString(getKey(@"required_mods_button_tooltip"), @"Choose the mods which all players should play with.");

        /// <summary>
        /// "Each player can choose their preferred mods from a selected list."
        /// </summary>
        public static LocalisableString FreeModsButtonTooltip => new TranslatableString(getKey(@"free_mods_button_tooltip"), @"Each player can choose their preferred mods from a selected list.");

        /// <summary>
        /// "Each player can choose their preferred difficulty, ruleset and mods."
        /// </summary>
        public static LocalisableString FreestyleButtonTooltip => new TranslatableString(getKey(@"freestyle_button_tooltip"), @"Each player can choose their preferred difficulty, ruleset and mods.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
