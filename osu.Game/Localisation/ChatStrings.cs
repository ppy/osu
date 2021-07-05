// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ChatStrings
    {
        private const string prefix = @"osu.Game.Localisation.Chat";

        /// <summary>
        /// "chat"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"chat");

        /// <summary>
        /// "join the real-time discussion"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"join the real-time discussion");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
