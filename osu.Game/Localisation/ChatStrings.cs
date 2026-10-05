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

        /// <summary>
        /// "press {0} to chat..."
        /// </summary>
        public static LocalisableString InGameInputPlaceholder(LocalisableString keyBind) => new TranslatableString(getKey(@"in_game_input_placeholder"), @"press {0} to chat...", keyBind);

        /// <summary>
        /// "Chat moderators have been alerted. Thanks for your help."
        /// </summary>
        public static LocalisableString ReportConfirmation => new TranslatableString(getKey(@"report_confirmation"), @"Chat moderators have been alerted. Thanks for your help.");

        /// <summary>
        /// "Chat moderators have been alerted. You have reported a private message so they will not be able to read history to maintain your privacy. Please make sure to include as much details as you can.
        /// You can submit a second report with more details if required, or contact abuse@ppy.sh if a user is being extremely offensive.
        /// You can also block a user via the block button on their user profile, or by right-clicking on their name in the chat and selecting &quot;Block&quot;."
        /// </summary>
        public static LocalisableString ReportConfirmationPM => new TranslatableString(getKey(@"report_confirmation_pm"), """
                                                                                                                          Chat moderators have been alerted. You have reported a private message so they will not be able to read history to maintain your privacy. Please make sure to include as much details as you can.
                                                                                                                          You can submit a second report with more details if required, or contact abuse@ppy.sh if a user is being extremely offensive.
                                                                                                                          You can also block a user via the block button on their user profile, or by right-clicking on their name in the chat and selecting "Block".
                                                                                                                          """);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
