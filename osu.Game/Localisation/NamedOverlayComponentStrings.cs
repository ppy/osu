// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class NamedOverlayComponentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.NamedOverlayComponent";

        /// <summary>
        /// "browse for new beatmaps"
        /// </summary>
        public static LocalisableString BeatmapListingDescription => new TranslatableString(getKey(@"beatmap_listing_description"), @"browse for new beatmaps");

        /// <summary>
        /// "track recent dev updates in the osu! ecosystem"
        /// </summary>
        public static LocalisableString ChangelogDescription => new TranslatableString(getKey(@"changelog_description"), @"track recent dev updates in the osu! ecosystem");

        /// <summary>
        /// "view your friends and spectate other players"
        /// </summary>
        public static LocalisableString DashboardDescription => new TranslatableString(getKey(@"dashboard_description"), @"view your friends and spectate other players");

        /// <summary>
        /// "find out who&#39;s the best right now"
        /// </summary>
        public static LocalisableString RankingsDescription => new TranslatableString(getKey(@"rankings_description"), @"find out who's the best right now");

        /// <summary>
        /// "get up-to-date on community happenings"
        /// </summary>
        public static LocalisableString NewsDescription => new TranslatableString(getKey(@"news_description"), @"get up-to-date on community happenings");

        /// <summary>
        /// "knowledge base"
        /// </summary>
        public static LocalisableString WikiDescription => new TranslatableString(getKey(@"wiki_description"), @"knowledge base");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
