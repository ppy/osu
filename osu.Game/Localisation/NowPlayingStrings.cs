// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class NowPlayingStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.NowPlaying";

        /// <summary>
        /// "now playing"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"now playing");

        /// <summary>
        /// "manage the currently playing track"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"manage the currently playing track");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
