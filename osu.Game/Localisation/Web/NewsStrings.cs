// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class NewsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.News";

        /// <summary>
        /// "osu!news"
        /// </summary>
        public static LocalisableString IndexTitlePage => new TranslatableString(getKey(@"index.title_page"), @"osu!news");

        /// <summary>
        /// "Newer posts"
        /// </summary>
        public static LocalisableString IndexNavNewer => new TranslatableString(getKey(@"index.nav.newer"), @"Newer posts");

        /// <summary>
        /// "Older posts"
        /// </summary>
        public static LocalisableString IndexNavOlder => new TranslatableString(getKey(@"index.nav.older"), @"Older posts");

        /// <summary>
        /// "news"
        /// </summary>
        public static LocalisableString IndexTitleDefault => new TranslatableString(getKey(@"index.title._"), @"news");

        /// <summary>
        /// "frontpage"
        /// </summary>
        public static LocalisableString IndexTitleInfo => new TranslatableString(getKey(@"index.title.info"), @"frontpage");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString ShowBy(string user) => new TranslatableString(getKey(@"show.by"), @"by {0}", user);

        /// <summary>
        /// "Newer post"
        /// </summary>
        public static LocalisableString ShowNavNewer => new TranslatableString(getKey(@"show.nav.newer"), @"Newer post");

        /// <summary>
        /// "Older post"
        /// </summary>
        public static LocalisableString ShowNavOlder => new TranslatableString(getKey(@"show.nav.older"), @"Older post");

        /// <summary>
        /// "news"
        /// </summary>
        public static LocalisableString ShowTitleDefault => new TranslatableString(getKey(@"show.title._"), @"news");

        /// <summary>
        /// "post"
        /// </summary>
        public static LocalisableString ShowTitleInfo => new TranslatableString(getKey(@"show.title.info"), @"post");

        /// <summary>
        /// "News Archive"
        /// </summary>
        public static LocalisableString SidebarArchive => new TranslatableString(getKey(@"sidebar.archive"), @"News Archive");

        /// <summary>
        /// "Update"
        /// </summary>
        public static LocalisableString StoreButton => new TranslatableString(getKey(@"store.button"), @"Update");

        /// <summary>
        /// "Listing updated."
        /// </summary>
        public static LocalisableString StoreOk => new TranslatableString(getKey(@"store.ok"), @"Listing updated.");

        /// <summary>
        /// "Update"
        /// </summary>
        public static LocalisableString UpdateButton => new TranslatableString(getKey(@"update.button"), @"Update");

        /// <summary>
        /// "Post updated."
        /// </summary>
        public static LocalisableString UpdateOk => new TranslatableString(getKey(@"update.ok"), @"Post updated.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}