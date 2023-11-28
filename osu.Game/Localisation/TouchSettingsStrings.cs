// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class TouchSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.TouchSettings";

        /// <summary>
        /// "Touch"
        /// </summary>
        public static LocalisableString Touch => new TranslatableString(getKey(@"touch"), @"Touch");

        /// <summary>
        /// "Disable taps during gameplay"
        /// </summary>
        public static LocalisableString DisableTapsDuringGameplay => new TranslatableString(getKey(@"disable_taps_during_gameplay"), @"Disable taps during gameplay");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}