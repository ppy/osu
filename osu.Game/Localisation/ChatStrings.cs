// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ChatStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Chat";

        /// <summary>
        /// "chat"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"chat");

        /// <summary>
        /// "join the real-time discussion"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"join the real-time discussion");

        /// <summary>
        /// "Mention"
        /// </summary>
        public static LocalisableString MentionUser => new TranslatableString(getKey(@"mention_user"), @"Mention");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
