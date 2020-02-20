// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchParameters
    {
        public readonly string Query;

        public readonly RulesetInfo Ruleset;

        public readonly BeatmapSearchCategory Category;

        public readonly BeatmapSearchGenre Genre;

        public readonly BeatmapSearchLanguage Language;

        public BeatmapSearchParameters(string query, RulesetInfo ruleset, BeatmapSearchCategory category, BeatmapSearchGenre genre, BeatmapSearchLanguage language)
        {
            Query = query;
            Ruleset = ruleset;
            Category = category;
            Genre = genre;
            Language = language;
        }
    }
}
