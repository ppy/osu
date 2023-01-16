// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class AccountCreationStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.AccountCreation";

        /// <summary>
        /// "New Player Registration"
        /// </summary>
        public static LocalisableString NewPlayerRegistration => new TranslatableString(getKey(@"new_player_registration"), @"New Player Registration");

        /// <summary>
        /// "let&#39;s get you started"
        /// </summary>
        public static LocalisableString LetsGetYouStarted => new TranslatableString(getKey(@"lets_get_you_started"), @"let's get you started");

        /// <summary>
        /// "Let&#39;s create an account!"
        /// </summary>
        public static LocalisableString LetsCreateAnAccount => new TranslatableString(getKey(@"lets_create_an_account"), @"Let's create an account!");

        /// <summary>
        /// "Help, I can&#39;t access my account!"
        /// </summary>
        public static LocalisableString HelpICantAccess => new TranslatableString(getKey(@"help_icant_access"), @"Help, I can't access my account!");

        /// <summary>
        /// "I understand. This account isn&#39;t for me."
        /// </summary>
        public static LocalisableString AccountIsntForMe => new TranslatableString(getKey(@"account_isnt_for_me"), @"I understand. This account isn't for me.");

        /// <summary>
        /// "This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!"
        /// </summary>
        public static LocalisableString ThisWillBeYourPublic => new TranslatableString(getKey(@"this_will_be_your_public"),
            @"This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!");

        /// <summary>
        /// "Will be used for notifications, account verification and in the case you forget your password. No spam, ever."
        /// </summary>
        public static LocalisableString EmailUsage =>
            new TranslatableString(getKey(@"email_usage"), @"Will be used for notifications, account verification and in the case you forget your password. No spam, ever.");

        /// <summary>
        /// " Make sure to get it right!"
        /// </summary>
        public static LocalisableString MakeSureToGetIt => new TranslatableString(getKey(@"make_sure_to_get_it"), @" Make sure to get it right!");

        /// <summary>
        /// "At least "
        /// </summary>
        public static LocalisableString BeforeCharactersLong => new TranslatableString(getKey(@"before_characters_long"), @"At least ");

        /// <summary>
        /// "8 characters long"
        /// </summary>
        public static LocalisableString CharactersLong => new TranslatableString(getKey(@"characters_long"), @"8 characters long");

        /// <summary>
        /// ". Choose something long but also something you will remember, like a line from your favourite song."
        /// </summary>
        public static LocalisableString AfterCharactersLong =>
            new TranslatableString(getKey(@"after_characters_long"), @". Choose something long but also something you will remember, like a line from your favourite song.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
