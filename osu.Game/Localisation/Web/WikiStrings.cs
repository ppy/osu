// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class WikiStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Wiki";

        /// <summary>
        /// "Requested page is not yet translated to the selected language ({0}). Showing English version."
        /// </summary>
        public static LocalisableString ShowFallbackTranslation(string language) => new TranslatableString(getKey(@"show.fallback_translation"), @"Requested page is not yet translated to the selected language ({0}). Showing English version.", language);

        /// <summary>
        /// "The content on this page is incomplete or outdated. If you are able to help out, please consider updating the article!"
        /// </summary>
        public static LocalisableString ShowIncompleteOrOutdated => new TranslatableString(getKey(@"show.incomplete_or_outdated"), @"The content on this page is incomplete or outdated. If you are able to help out, please consider updating the article!");

        /// <summary>
        /// "Requested page &quot;{0}&quot; could not be found."
        /// </summary>
        public static LocalisableString ShowMissing(string keyword) => new TranslatableString(getKey(@"show.missing"), @"Requested page ""{0}"" could not be found.", keyword);

        /// <summary>
        /// "Not Found"
        /// </summary>
        public static LocalisableString ShowMissingTitle => new TranslatableString(getKey(@"show.missing_title"), @"Not Found");

        /// <summary>
        /// "Requested page could not be found for currently selected language."
        /// </summary>
        public static LocalisableString ShowMissingTranslation => new TranslatableString(getKey(@"show.missing_translation"), @"Requested page could not be found for currently selected language.");

        /// <summary>
        /// "This page does not meet the standards of the osu! wiki and needs to be cleaned up or rewritten. If you are able to help out, please consider updating the article!"
        /// </summary>
        public static LocalisableString ShowNeedsCleanupOrRewrite => new TranslatableString(getKey(@"show.needs_cleanup_or_rewrite"), @"This page does not meet the standards of the osu! wiki and needs to be cleaned up or rewritten. If you are able to help out, please consider updating the article!");

        /// <summary>
        /// "Search existing pages for {0}."
        /// </summary>
        public static LocalisableString ShowSearch(string link) => new TranslatableString(getKey(@"show.search"), @"Search existing pages for {0}.", link);

        /// <summary>
        /// "Contents"
        /// </summary>
        public static LocalisableString ShowToc => new TranslatableString(getKey(@"show.toc"), @"Contents");

        /// <summary>
        /// "Show on GitHub"
        /// </summary>
        public static LocalisableString ShowEditLink => new TranslatableString(getKey(@"show.edit.link"), @"Show on GitHub");

        /// <summary>
        /// "Refresh"
        /// </summary>
        public static LocalisableString ShowEditRefresh => new TranslatableString(getKey(@"show.edit.refresh"), @"Refresh");

        /// <summary>
        /// "This translation is provided for convenience only. The original {0} shall be the sole legally binding version of this text."
        /// </summary>
        public static LocalisableString ShowTranslationLegal(string @default) => new TranslatableString(getKey(@"show.translation.legal"), @"This translation is provided for convenience only. The original {0} shall be the sole legally binding version of this text.", @default);

        /// <summary>
        /// "This page contains an outdated translation of the original content. Please check the {0} for the most accurate information (and consider updating the translation if you are able to help out)!"
        /// </summary>
        public static LocalisableString ShowTranslationOutdated(string @default) => new TranslatableString(getKey(@"show.translation.outdated"), @"This page contains an outdated translation of the original content. Please check the {0} for the most accurate information (and consider updating the translation if you are able to help out)!", @default);

        /// <summary>
        /// "English version"
        /// </summary>
        public static LocalisableString ShowTranslationDefault => new TranslatableString(getKey(@"show.translation.default"), @"English version");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}