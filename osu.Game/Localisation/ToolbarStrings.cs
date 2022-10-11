// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ToolbarStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Toolbar";

        /// <summary>
        /// "Connection interrupted, will try to reconnect..."
        /// </summary>
        public static LocalisableString AttemptingToReconnect => new TranslatableString(getKey(@"attempting_to_reconnect"), @"Connection interrupted, will try to reconnect...");

        /// <summary>
        /// "Connecting..."
        /// </summary>
        public static LocalisableString Connecting => new TranslatableString(getKey(@"connecting"), @"Connecting...");

        /// <summary>
        /// "home"
        /// </summary>
        public static LocalisableString HomeHeaderTitle => new TranslatableString(getKey(@"header_title"), @"home");

        /// <summary>
        /// "return to the main menu"
        /// </summary>
        public static LocalisableString HomeHeaderDescription => new TranslatableString(getKey(@"header_description"), @"return to the main menu");

        /// <summary>
        /// "play some"
        /// </summary>

        public static LocalisableString RulesetHeaderDescription => new TranslatableString(getKey(@"header_description"), @"play some");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
