// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ScoresStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Scores";

        /// <summary>
        /// "{0} on {1} [{2}]"
        /// </summary>
        public static LocalisableString ShowTitle(string username, string title, string version) => new TranslatableString(getKey(@"show.title"), @"{0} on {1} [{2}]", username, title, version);

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString ShowBeatmapBy(string artist) => new TranslatableString(getKey(@"show.beatmap.by"), @"by {0}", artist);

        /// <summary>
        /// "Played by"
        /// </summary>
        public static LocalisableString ShowPlayerBy => new TranslatableString(getKey(@"show.player.by"), @"Played by");

        /// <summary>
        /// "Submitted on"
        /// </summary>
        public static LocalisableString ShowPlayerSubmittedOn => new TranslatableString(getKey(@"show.player.submitted_on"), @"Submitted on");

        /// <summary>
        /// "Country Rank"
        /// </summary>
        public static LocalisableString ShowPlayerRankCountry => new TranslatableString(getKey(@"show.player.rank.country"), @"Country Rank");

        /// <summary>
        /// "Global Rank"
        /// </summary>
        public static LocalisableString ShowPlayerRankGlobal => new TranslatableString(getKey(@"show.player.rank.global"), @"Global Rank");

        /// <summary>
        /// "Only personal best scores award pp"
        /// </summary>
        public static LocalisableString StatusNonBest => new TranslatableString(getKey(@"status.non_best"), @"Only personal best scores award pp");

        /// <summary>
        /// "This score is still being calculated and will be displayed soon"
        /// </summary>
        public static LocalisableString StatusProcessing => new TranslatableString(getKey(@"status.processing"), @"This score is still being calculated and will be displayed soon");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}