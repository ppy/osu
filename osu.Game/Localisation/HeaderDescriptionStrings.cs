// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class HeaderDescriptionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HeaderDescription";

        /// <summary>
        /// "browse for new beatmaps"
        /// </summary>
        public static LocalisableString BeatmapListing => new TranslatableString(getKey(@"beatmap_listing"), @"browse for new beatmaps");

        /// <summary>
        /// "track recent dev updates in the osu! ecosystem"
        /// </summary>
        public static LocalisableString Changelog => new TranslatableString(getKey(@"changelog"), @"track recent dev updates in the osu! ecosystem");

        /// <summary>
        /// "view your friends and other information"
        /// </summary>
        public static LocalisableString Dashboard => new TranslatableString(getKey(@"dashboard"), @"view your friends and other information");

        /// <summary>
        /// "find out who's the best right now"
        /// </summary>
        public static LocalisableString Rankings => new TranslatableString(getKey(@"rankings"), @"find out who's the best right now");

        /// <summary>
        /// "get up-to-date on community happenings"
        /// </summary>
        public static LocalisableString News => new TranslatableString(getKey(@"news"), @"get up-to-date on community happenings");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
