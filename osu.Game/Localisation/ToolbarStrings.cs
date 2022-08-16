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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
