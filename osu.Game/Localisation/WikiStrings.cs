// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class WikiStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Wiki";

        /// <summary>
        /// "index"
        /// </summary>
        public static LocalisableString IndexPageString => new TranslatableString(getKey(@"index_page"), @"index");

        /// <summary>
        /// "wiki"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"wiki");

        /// <summary>
        /// "knowledge base"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"knowledge base");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
