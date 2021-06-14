// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ClientVerificationsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.ClientVerifications";

        /// <summary>
        /// "Go to dashboard"
        /// </summary>
        public static LocalisableString CompletedHome => new TranslatableString(getKey(@"completed.home"), @"Go to dashboard");

        /// <summary>
        /// "Logout"
        /// </summary>
        public static LocalisableString CompletedLogout => new TranslatableString(getKey(@"completed.logout"), @"Logout");

        /// <summary>
        /// "You can close this tab/window now"
        /// </summary>
        public static LocalisableString CompletedText => new TranslatableString(getKey(@"completed.text"), @"You can close this tab/window now");

        /// <summary>
        /// "osu! client verification has been completed"
        /// </summary>
        public static LocalisableString CompletedTitle => new TranslatableString(getKey(@"completed.title"), @"osu! client verification has been completed");

        /// <summary>
        /// "Click on authorise button below to finish client verification."
        /// </summary>
        public static LocalisableString CreateConfirm => new TranslatableString(getKey(@"create.confirm"), @"Click on authorise button below to finish client verification.");

        /// <summary>
        /// "osu! client verification"
        /// </summary>
        public static LocalisableString CreateTitle => new TranslatableString(getKey(@"create.title"), @"osu! client verification");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}