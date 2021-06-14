// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web.ModelValidation
{
    public static class FulfillmentsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.ModelValidation.Fulfillments";

        /// <summary>
        /// "only 1 username change allowed per order fulfillment."
        /// </summary>
        public static LocalisableString UsernameChangeOnlyOne => new TranslatableString(getKey(@"username_change.only_one"), @"only 1 username change allowed per order fulfillment.");

        /// <summary>
        /// "Username change cost exceeds amount paid ({0} &gt; {1})"
        /// </summary>
        public static LocalisableString UsernameChangeInsufficientPaid(string expected, string actual) => new TranslatableString(getKey(@"username_change.insufficient_paid"), @"Username change cost exceeds amount paid ({0} > {1})", expected, actual);

        /// <summary>
        /// "Current username ({0}) is not the same as change to revoke ({1})"
        /// </summary>
        public static LocalisableString UsernameChangeRevertingUsernameMismatch(string current, string username) => new TranslatableString(getKey(@"username_change.reverting_username_mismatch"), @"Current username ({0}) is not the same as change to revoke ({1})", current, username);

        /// <summary>
        /// "Donation is less than required for osu!supporter tag gift ({0} &gt; {1})"
        /// </summary>
        public static LocalisableString SupporterTagInsufficientPaid(string actual, string expected) => new TranslatableString(getKey(@"supporter_tag.insufficient_paid"), @"Donation is less than required for osu!supporter tag gift ({0} > {1})", actual, expected);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}