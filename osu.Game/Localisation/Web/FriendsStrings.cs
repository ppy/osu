// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class FriendsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Friends";

        /// <summary>
        /// "friends"
        /// </summary>
        public static LocalisableString TitleCompact => new TranslatableString(getKey(@"title_compact"), @"friends");

        /// <summary>
        /// "Friend limit reached"
        /// </summary>
        public static LocalisableString TooMany => new TranslatableString(getKey(@"too_many"), @"Friend limit reached");

        /// <summary>
        /// "add friend"
        /// </summary>
        public static LocalisableString ButtonsAdd => new TranslatableString(getKey(@"buttons.add"), @"add friend");

        /// <summary>
        /// "followers"
        /// </summary>
        public static LocalisableString ButtonsDisabled => new TranslatableString(getKey(@"buttons.disabled"), @"followers");

        /// <summary>
        /// "remove friend"
        /// </summary>
        public static LocalisableString ButtonsRemove => new TranslatableString(getKey(@"buttons.remove"), @"remove friend");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}