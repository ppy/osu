// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ChangelogStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Changelog";

        /// <summary>
        /// "Bug fixes and minor improvements"
        /// </summary>
        public static LocalisableString Generic => new TranslatableString(getKey(@"generic"), @"Bug fixes and minor improvements");

        /// <summary>
        /// "changes in {0}"
        /// </summary>
        public static LocalisableString BuildTitle(string version) => new TranslatableString(getKey(@"build.title"), @"changes in {0}", version);

        /// <summary>
        /// "{0} user online|{0} users online"
        /// </summary>
        public static LocalisableString BuildsUsersOnline(string countDelimited) => new TranslatableString(getKey(@"builds.users_online"), @"{0} user online|{0} users online", countDelimited);

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString EntryBy(string user) => new TranslatableString(getKey(@"entry.by"), @"by {0}", user);

        /// <summary>
        /// "changelog listing"
        /// </summary>
        public static LocalisableString IndexPageTitleDefault => new TranslatableString(getKey(@"index.page_title._"), @"changelog listing");

        /// <summary>
        /// "changes since {0}"
        /// </summary>
        public static LocalisableString IndexPageTitleFrom(string from) => new TranslatableString(getKey(@"index.page_title._from"), @"changes since {0}", from);

        /// <summary>
        /// "changes between {0} and {1}"
        /// </summary>
        public static LocalisableString IndexPageTitleFromTo(string from, string to) => new TranslatableString(getKey(@"index.page_title._from_to"), @"changes between {0} and {1}", from, to);

        /// <summary>
        /// "changes in {0}"
        /// </summary>
        public static LocalisableString IndexPageTitleStream(string stream) => new TranslatableString(getKey(@"index.page_title._stream"), @"changes in {0}", stream);

        /// <summary>
        /// "changes in {0} since {1}"
        /// </summary>
        public static LocalisableString IndexPageTitleStreamFrom(string stream, string from) => new TranslatableString(getKey(@"index.page_title._stream_from"), @"changes in {0} since {1}", stream, from);

        /// <summary>
        /// "changes in {0} between {1} and {2}"
        /// </summary>
        public static LocalisableString IndexPageTitleStreamFromTo(string stream, string from, string to) => new TranslatableString(getKey(@"index.page_title._stream_from_to"), @"changes in {0} between {1} and {2}", stream, from, to);

        /// <summary>
        /// "changes in {0} up to {1}"
        /// </summary>
        public static LocalisableString IndexPageTitleStreamTo(string stream, string to) => new TranslatableString(getKey(@"index.page_title._stream_to"), @"changes in {0} up to {1}", stream, to);

        /// <summary>
        /// "changes up to {0}"
        /// </summary>
        public static LocalisableString IndexPageTitleTo(string to) => new TranslatableString(getKey(@"index.page_title._to"), @"changes up to {0}", to);

        /// <summary>
        /// "Love this update?"
        /// </summary>
        public static LocalisableString SupportHeading => new TranslatableString(getKey(@"support.heading"), @"Love this update?");

        /// <summary>
        /// "Support further development of osu! and {0} today!"
        /// </summary>
        public static LocalisableString SupportText1(string link) => new TranslatableString(getKey(@"support.text_1"), @"Support further development of osu! and {0} today!", link);

        /// <summary>
        /// "become an osu!supporter"
        /// </summary>
        public static LocalisableString SupportText1Link => new TranslatableString(getKey(@"support.text_1_link"), @"become an osu!supporter");

        /// <summary>
        /// "Not only will you help speed development, but you will also get some extra features and customisations!"
        /// </summary>
        public static LocalisableString SupportText2 => new TranslatableString(getKey(@"support.text_2"), @"Not only will you help speed development, but you will also get some extra features and customisations!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}