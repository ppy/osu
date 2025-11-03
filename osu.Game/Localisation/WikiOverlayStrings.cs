// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class WikiOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.WikiOverlay";

        /// <summary>
        /// "Show on GitHub"
        /// </summary>
        public static LocalisableString ShowOnGitHub => new TranslatableString(getKey(@"show_on_github"), @"Show on GitHub");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
