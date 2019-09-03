// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.IO.Network;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        private readonly string query;
        private readonly int page;
        private readonly RulesetInfo ruleset;
        private readonly BeatmapSearchCategory searchCategory;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection direction;
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, int page = 1, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
        {
            this.query = System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.page = page;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
        }

        protected override string Target => @"beatmapsets/search";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("q", query);
            req.AddParameter("m", $"{ruleset.ID ?? 0}");
            req.AddParameter("s", searchCategory.ToString().ToLowerInvariant());
            req.AddParameter("sort", $"{sortCriteria.ToString().ToLowerInvariant()}_{directionString}");
            req.AddParameter("page", page.ToString());

            return req;
        }
    }

    public enum BeatmapSearchCategory
    {
        Any,

        [Description("Has Leaderboard")]
        Leaderboard,
        Ranked,
        Qualified,
        Loved,
        Favourites,

        [Description("Pending & WIP")]
        Pending,
        Graveyard,

        [Description("My Maps")]
        Mine,
    }
}
