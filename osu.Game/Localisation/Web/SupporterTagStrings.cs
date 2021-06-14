// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class SupporterTagStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.SupporterTag";

        /// <summary>
        /// "months"
        /// </summary>
        public static LocalisableString Months => new TranslatableString(getKey(@"months"), @"months");

        /// <summary>
        /// "searching..."
        /// </summary>
        public static LocalisableString UserSearchSearching => new TranslatableString(getKey(@"user_search.searching"), @"searching...");

        /// <summary>
        /// "This user doesn&#39;t exist"
        /// </summary>
        public static LocalisableString UserSearchNotFound => new TranslatableString(getKey(@"user_search.not_found"), @"This user doesn't exist");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}