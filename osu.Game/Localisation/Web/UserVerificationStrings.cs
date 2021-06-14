// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class UserVerificationStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.UserVerification";

        /// <summary>
        /// "An email has been sent to {0} with a verification code. Enter the code."
        /// </summary>
        public static LocalisableString BoxSent(string mail) => new TranslatableString(getKey(@"box.sent"), @"An email has been sent to {0} with a verification code. Enter the code.", mail);

        /// <summary>
        /// "Account Verification"
        /// </summary>
        public static LocalisableString BoxTitle => new TranslatableString(getKey(@"box.title"), @"Account Verification");

        /// <summary>
        /// "Verifying..."
        /// </summary>
        public static LocalisableString BoxVerifying => new TranslatableString(getKey(@"box.verifying"), @"Verifying...");

        /// <summary>
        /// "Issuing new code..."
        /// </summary>
        public static LocalisableString BoxIssuing => new TranslatableString(getKey(@"box.issuing"), @"Issuing new code...");

        /// <summary>
        /// "Make sure to check your spam folder if you can&#39;t find the email."
        /// </summary>
        public static LocalisableString BoxInfoCheckSpam => new TranslatableString(getKey(@"box.info.check_spam"), @"Make sure to check your spam folder if you can't find the email.");

        /// <summary>
        /// "If you can&#39;t access your email or have forgotten what you used, please follow the {0}."
        /// </summary>
        public static LocalisableString BoxInfoRecover(string link) => new TranslatableString(getKey(@"box.info.recover"), @"If you can't access your email or have forgotten what you used, please follow the {0}.", link);

        /// <summary>
        /// "email recovery process here"
        /// </summary>
        public static LocalisableString BoxInfoRecoverLink => new TranslatableString(getKey(@"box.info.recover_link"), @"email recovery process here");

        /// <summary>
        /// "You can also {0} or {1}."
        /// </summary>
        public static LocalisableString BoxInfoReissue(string reissueLink, string logoutLink) => new TranslatableString(getKey(@"box.info.reissue"), @"You can also {0} or {1}.", reissueLink, logoutLink);

        /// <summary>
        /// "request another code"
        /// </summary>
        public static LocalisableString BoxInfoReissueLink => new TranslatableString(getKey(@"box.info.reissue_link"), @"request another code");

        /// <summary>
        /// "sign out"
        /// </summary>
        public static LocalisableString BoxInfoLogoutLink => new TranslatableString(getKey(@"box.info.logout_link"), @"sign out");

        /// <summary>
        /// "Verification code expired, new verification email sent."
        /// </summary>
        public static LocalisableString ErrorsExpired => new TranslatableString(getKey(@"errors.expired"), @"Verification code expired, new verification email sent.");

        /// <summary>
        /// "Incorrect verification code."
        /// </summary>
        public static LocalisableString ErrorsIncorrectKey => new TranslatableString(getKey(@"errors.incorrect_key"), @"Incorrect verification code.");

        /// <summary>
        /// "Incorrect verification code. Retry limit exceeded, new verification email sent."
        /// </summary>
        public static LocalisableString ErrorsRetriesExceeded => new TranslatableString(getKey(@"errors.retries_exceeded"), @"Incorrect verification code. Retry limit exceeded, new verification email sent.");

        /// <summary>
        /// "Verification code reissued, new verification email sent."
        /// </summary>
        public static LocalisableString ErrorsReissued => new TranslatableString(getKey(@"errors.reissued"), @"Verification code reissued, new verification email sent.");

        /// <summary>
        /// "Unknown problem occurred, new verification email sent."
        /// </summary>
        public static LocalisableString ErrorsUnknown => new TranslatableString(getKey(@"errors.unknown"), @"Unknown problem occurred, new verification email sent.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}