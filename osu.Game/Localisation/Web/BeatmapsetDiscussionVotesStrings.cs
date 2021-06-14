// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapsetDiscussionVotesStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.BeatmapsetDiscussionVotes";

        /// <summary>
        /// "Beatmap Discussion Votes"
        /// </summary>
        public static LocalisableString IndexTitle => new TranslatableString(getKey(@"index.title"), @"Beatmap Discussion Votes");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString ItemScore => new TranslatableString(getKey(@"item.score"), @"Score");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}