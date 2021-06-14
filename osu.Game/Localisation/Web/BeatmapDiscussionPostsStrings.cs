// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapDiscussionPostsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.BeatmapDiscussionPosts";

        /// <summary>
        /// "Beatmap Discussion Posts"
        /// </summary>
        public static LocalisableString IndexTitle => new TranslatableString(getKey(@"index.title"), @"Beatmap Discussion Posts");

        /// <summary>
        /// "Content"
        /// </summary>
        public static LocalisableString ItemContent => new TranslatableString(getKey(@"item.content"), @"Content");

        /// <summary>
        /// "View modding history"
        /// </summary>
        public static LocalisableString ItemModdingHistoryLink => new TranslatableString(getKey(@"item.modding_history_link"), @"View modding history");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}