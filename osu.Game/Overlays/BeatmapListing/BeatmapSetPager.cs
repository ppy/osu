// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSetPager
    {
        private readonly IAPIProvider API;

        public event PageFetchHandler PageFetch;

        private readonly RulesetStore rulesets;

        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly BeatmapSearchCategory searchCategory;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection sortDirection;

        private SearchBeatmapSetsRequest getSetsRequest;

        private SearchBeatmapSetsResponse lastResponse;

        public int? TotalSets => lastResponse?.Total;
        public bool IsPastFirstPage { get; private set; } = false;
        public bool IsLastPageFetched { get; private set; } = false;
        public bool IsFetching => getSetsRequest != null;
        public bool CanFetchNextPage => !IsLastPageFetched && !IsFetching;

        public BeatmapSetPager(IAPIProvider API, RulesetStore rulesets, string query, RulesetInfo ruleset, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection sortDirection = SortDirection.Descending)
        {
            this.API = API;

            this.rulesets = rulesets;

            this.query = query;
            this.ruleset = ruleset;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.sortDirection = sortDirection;
        }

        /// <summary>
        /// Fetches the next page of beatmap sets. This method is not thread-safe.
        /// </summary>
        public void FetchNextPage()
        {
            if (IsFetching)
                return;

            if (lastResponse != null)
                IsPastFirstPage = true;

            getSetsRequest = new SearchBeatmapSetsRequest(
                query,
                ruleset,
                lastResponse?.CursorJson,
                searchCategory,
                sortCriteria,
                sortDirection);

            getSetsRequest.Success += response =>
            {
                var sets = response.BeatmapSets.Select(r => r.ToBeatmapSet(rulesets)).ToList();

                if (sets.Count == 0) IsLastPageFetched = true;

                lastResponse = response;
                getSetsRequest = null;

                PageFetch?.Invoke(sets);
            };

            API.Queue(getSetsRequest);
        }

        public void Reset()
        {
            IsLastPageFetched = false;

            lastResponse = null;

            getSetsRequest?.Cancel();
            getSetsRequest = null;
        }

        public delegate void PageFetchHandler(List<BeatmapSetInfo> sets);
    }
}
