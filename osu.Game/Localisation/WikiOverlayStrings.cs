// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class WikiOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.WikiOverlayStrings";

        /// <summary>
        /// "Something went wrong when trying to fetch page "{0}"."
        /// </summary>
        public static LocalisableString PageErrorDescription(string path) =>
            new TranslatableString(getKey(@"page_error_description"), @"Something went wrong when trying to fetch page ""{0}"".", path);

        /// <summary>
        /// "[Reload this page]({0})."
        /// </summary>
        public static LocalisableString ReloadPageLink(string url) => new TranslatableString(getKey(@"reload_page_link"), @"[Reload this page]({0}).", url);

        /// <summary>
        /// "[Return to the main page]({0})."
        /// </summary>
        public static LocalisableString ReturnToMainPageLink(string url) => new TranslatableString(getKey(@"return_to_main_page_link"), @"[Return to the main page]({0}).", url);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
