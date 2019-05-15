// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly BeatmapSearchCategory searchCategory;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection direction;
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
        {
            this.query = System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"beatmapsets/search?q={query}&m={ruleset.ID ?? 0}&s={(int)searchCategory}&sort={sortCriteria.ToString().ToLowerInvariant()}_{directionString}";
    }

    public enum BeatmapSearchCategory
    {
        Any = 7,

        [Description("Ranked & Approved")]
        RankedApproved = 0,
        Qualified = 3,
        Loved = 8,
        Favourites = 2,

        [Description("Pending & WIP")]
        PendingWIP = 4,
        Graveyard = 5,

        [Description("My Maps")]
        MyMaps = 6,
    }
}
