// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class JudgementCounterDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.JudgementCounterDisplay";

        /// <summary>
        /// "Display mode"
        /// </summary>
        public static LocalisableString JudgementDisplayMode => new TranslatableString(getKey(@"judgement_display_mode"), "Display mode");

        /// <summary>
        /// "Counter direction"
        /// </summary>
        public static LocalisableString FlowDirection => new TranslatableString(getKey(@"flow_direction"), "Counter direction");

        /// <summary>
        /// "Show judgement names"
        /// </summary>
        public static LocalisableString ShowJudgementNames => new TranslatableString(getKey(@"show_judgement_names"), "Show judgement names");

        /// <summary>
        /// "Show max judgement"
        /// </summary>
        public static LocalisableString ShowMaxJudgement => new TranslatableString(getKey(@"show_max_judgement"), "Show max judgement");

        /// <summary>
        /// "Simple"
        /// </summary>
        public static LocalisableString JudgementDisplayModeSimple => new TranslatableString(getKey(@"judgement_display_mode_simple"), "Simple");

        /// <summary>
        /// "Normal"
        /// </summary>
        public static LocalisableString JudgementDisplayModeNormal => new TranslatableString(getKey(@"judgement_display_mode_normal"), "Normal");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString JudgementDisplayModeAll => new TranslatableString(getKey(@"judgement_display_mode_all"), "All");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
