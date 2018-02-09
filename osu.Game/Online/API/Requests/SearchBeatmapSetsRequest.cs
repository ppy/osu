// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<IEnumerable<APIResponseBeatmapSet>>
    {
        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly RankStatus rankStatus;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection direction;
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, RankStatus rankStatus = RankStatus.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
        {
            this.query = System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.rankStatus = rankStatus;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"beatmapsets/search?q={query}&m={ruleset.ID ?? 0}&s={(int)rankStatus}&sort={sortCriteria.ToString().ToLower()}_{directionString}";
    }
}
