// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class GameplayAccuracyCounterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.GameplayAccuracyCounter";

        /// <summary>
        /// "Accuracy display mode"
        /// </summary>
        public static LocalisableString AccuracyDisplay => new TranslatableString(getKey(@"accuracy_display"), "Accuracy display mode");

        /// <summary>
        /// "Which accuracy mode should be displayed."
        /// </summary>
        public static LocalisableString AccuracyDisplayDescription => new TranslatableString(getKey(@"accuracy_display_description"), "Which accuracy mode should be displayed.");

        /// <summary>
        /// "Standard"
        /// </summary>
        public static LocalisableString AccuracyDisplayModeStandard => new TranslatableString(getKey(@"accuracy_display_mode_standard"), "Standard");

        /// <summary>
        /// "Maximum achievable"
        /// </summary>
        public static LocalisableString AccuracyDisplayModeMax => new TranslatableString(getKey(@"accuracy_display_mode_max"), "Maximum achievable");

        /// <summary>
        /// "Minimum achievable"
        /// </summary>
        public static LocalisableString AccuracyDisplayModeMin => new TranslatableString(getKey(@"accuracy_display_mode_min"), "Minimum achievable");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
