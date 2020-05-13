// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingPager
    {
        private readonly IAPIProvider api;
        private readonly RulesetStore rulesets;
        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly SearchCategory searchCategory;
        private readonly SortCriteria sortCriteria;
        private readonly SortDirection sortDirection;

        public event PageFetchHandler PageFetched;
        private SearchBeatmapSetsRequest getSetsRequest;
        private SearchBeatmapSetsResponse lastResponse;

        private bool isLastPageFetched;
        private bool isFetching => getSetsRequest != null;
        public bool IsPastFirstPage { get; private set; }
        public bool CanFetchNextPage => !isLastPageFetched && !isFetching;

        public BeatmapListingPager(IAPIProvider api, RulesetStore rulesets, string query, RulesetInfo ruleset, SearchCategory searchCategory = SearchCategory.Any, SortCriteria sortCriteria = SortCriteria.Ranked, SortDirection sortDirection = SortDirection.Descending)
        {
            this.api = api;
            this.rulesets = rulesets;
            this.query = query;
            this.ruleset = ruleset;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.sortDirection = sortDirection;
        }

        public void FetchNextPage()
        {
            if (isFetching)
                return;

            if (lastResponse != null)
                IsPastFirstPage = true;

            getSetsRequest = new SearchBeatmapSetsRequest(
                query,
                ruleset,
                lastResponse?.Cursor,
                searchCategory,
                sortCriteria,
                sortDirection);

            getSetsRequest.Success += response =>
            {
                var sets = response.BeatmapSets.Select(responseJson => responseJson.ToBeatmapSet(rulesets)).ToList();

                if (sets.Count == 0)
                    isLastPageFetched = true;

                lastResponse = response;
                getSetsRequest = null;

                PageFetched?.Invoke(sets);
            };

            api.Queue(getSetsRequest);
        }

        public void Reset()
        {
            isLastPageFetched = false;
            IsPastFirstPage = false;

            lastResponse = null;

            getSetsRequest?.Cancel();
            getSetsRequest = null;
        }

        public delegate void PageFetchHandler(List<BeatmapSetInfo> sets);
    }
}
