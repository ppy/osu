// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<IEnumerable<APIBeatmapSet>>
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
        protected override string Target => $@"beatmapsets/search?q={query}&m={ruleset.ID ?? 0}&s={(int)searchCategory}&sort={sortCriteria.ToString().ToLower()}_{directionString}";
    }

    public enum BeatmapSearchCategory
    {
        Any = 7,
        [Description("Ranked & Approved")]
        RankedApproved = 0,
        Approved = 1,
        Loved = 8,
        Favourites = 2,
        Qualified = 3,
        Pending = 4,
        Graveyard = 5,
        [Description("My Maps")]
        MyMaps = 6,
    }
}
