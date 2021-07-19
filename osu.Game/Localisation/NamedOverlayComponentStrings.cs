// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Localisation
{
    public static class NamedOverlayComponentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.NamedOverlayComponent";

        /// <inheritdoc cref="PageTitleStrings.MainBeatmapsetsControllerIndex"/>
        public static LocalisableString BeatmapListingTitle => PageTitleStrings.MainBeatmapsetsControllerIndex;

        /// <summary>
        /// "browse for new beatmaps"
        /// </summary>
        public static LocalisableString BeatmapListingDescription => new TranslatableString(getKey(@"beatmap_listing"), @"browse for new beatmaps");

        /// <inheritdoc cref="PageTitleStrings.MainChangelogControllerDefault"/>
        public static LocalisableString ChangelogTitle => PageTitleStrings.MainChangelogControllerDefault;

        /// <summary>
        /// "track recent dev updates in the osu! ecosystem"
        /// </summary>
        public static LocalisableString ChangelogDescription => new TranslatableString(getKey(@"changelog"), @"track recent dev updates in the osu! ecosystem");

        /// <inheritdoc cref="PageTitleStrings.MainHomeControllerIndex"/>
        public static LocalisableString DashboardTitle => PageTitleStrings.MainHomeControllerIndex;

        /// <summary>
        /// "view your friends and other information"
        /// </summary>
        public static LocalisableString DashboardDescription => new TranslatableString(getKey(@"dashboard"), @"view your friends and other information");

        /// <inheritdoc cref="PageTitleStrings.MainRankingControllerDefault"/>
        public static LocalisableString RankingsTitle => PageTitleStrings.MainRankingControllerDefault;

        /// <summary>
        /// "find out who's the best right now"
        /// </summary>
        public static LocalisableString RankingsDescription => new TranslatableString(getKey(@"rankings"), @"find out who's the best right now");

        /// <inheritdoc cref="PageTitleStrings.MainNewsControllerDefault"/>
        public static LocalisableString NewsTitle => PageTitleStrings.MainNewsControllerDefault;

        /// <summary>
        /// "get up-to-date on community happenings"
        /// </summary>
        public static LocalisableString NewsDescription => new TranslatableString(getKey(@"news"), @"get up-to-date on community happenings");

        /// <inheritdoc cref="LayoutStrings.MenuHelpGetWiki"/>
        public static LocalisableString WikiTitle => PageTitleStrings.MainWikiControllerDefault;

        /// <summary>
        /// "knowledge base"
        /// </summary>
        public static LocalisableString WikiDescription => new TranslatableString(getKey(@"wiki"), @"knowledge base");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
