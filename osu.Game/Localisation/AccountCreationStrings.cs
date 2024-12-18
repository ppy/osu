// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class AccountCreationStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.AccountCreation";

        /// <summary>
        /// "New player registration"
        /// </summary>
        public static LocalisableString NewPlayerRegistration => new TranslatableString(getKey(@"new_player_registration"), @"New player registration");

        /// <summary>
        /// "Let&#39;s get you started"
        /// </summary>
        public static LocalisableString LetsGetYouStarted => new TranslatableString(getKey(@"lets_get_you_started"), @"Let's get you started");

        /// <summary>
        /// "Let&#39;s create an account!"
        /// </summary>
        public static LocalisableString LetsCreateAnAccount => new TranslatableString(getKey(@"lets_create_an_account"), @"Let's create an account!");

        /// <summary>
        /// "Help, I can&#39;t access my account!"
        /// </summary>
        public static LocalisableString MultiAccountWarningHelp => new TranslatableString(getKey(@"multi_account_warning_help"), @"Help, I can't access my account!");

        /// <summary>
        /// "I understand. This account isn&#39;t for me."
        /// </summary>
        public static LocalisableString MultiAccountWarningAccept => new TranslatableString(getKey(@"multi_account_warning_accept"), @"I understand. This account isn't for me.");

        /// <summary>
        /// "This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!"
        /// </summary>
        public static LocalisableString UsernameDescription => new TranslatableString(getKey(@"username_description"), @"This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!");

        /// <summary>
        /// "Will be used for notifications, account verification and in the case you forget your password. No spam, ever."
        /// </summary>
        public static LocalisableString EmailDescription1 => new TranslatableString(getKey(@"email_description_1"), @"Will be used for notifications, account verification and in the case you forget your password. No spam, ever.");

        /// <summary>
        /// " Make sure to get it right!"
        /// </summary>
        public static LocalisableString EmailDescription2 => new TranslatableString(getKey(@"email_description_2"), @" Make sure to get it right!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
