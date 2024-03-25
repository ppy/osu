// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class WindowsCompatibilityModeCheckerStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.WindowsCompatibilityModeChecker";

        /// <summary>
        /// "osu! is running in compatibility mode. This may cause issues with the game. Please ensure osu! is not set to run in compatibility mode."
        /// </summary>
        public static LocalisableString NotificationText => new TranslatableString(getKey(@"notification_text"), @"osu! is running in compatibility mode. This may cause issues with the game. Please ensure osu! is not set to run in compatibility mode.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
