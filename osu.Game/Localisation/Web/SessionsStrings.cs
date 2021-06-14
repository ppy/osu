// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class SessionsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Sessions";

        /// <summary>
        /// "Click here to download the game and create an account"
        /// </summary>
        public static LocalisableString CreateDownload => new TranslatableString(getKey(@"create.download"), @"Click here to download the game and create an account");

        /// <summary>
        /// "First, let&#39;s sign into your account!"
        /// </summary>
        public static LocalisableString CreateLabel => new TranslatableString(getKey(@"create.label"), @"First, let's sign into your account!");

        /// <summary>
        /// "Account Sign-in"
        /// </summary>
        public static LocalisableString CreateTitle => new TranslatableString(getKey(@"create.title"), @"Account Sign-in");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}