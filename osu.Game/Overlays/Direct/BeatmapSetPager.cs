using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Direct
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

        private int currentPage = 1;

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

        public void FetchNextPage()
        {
            if (getSetsRequest != null)
                return;

            getSetsRequest = new SearchBeatmapSetsRequest(
                query,
                ruleset,
                currentPage,
                searchCategory,
                sortCriteria,
                sortDirection);

            lock (getSetsRequest) {
                getSetsRequest.Success += response =>
                {
                    var sets = response.BeatmapSets.Select(r => r.ToBeatmapSet(rulesets)).ToList();

                    if (sets.Count <= 0) IsLastPageFetched = true;

                    PageFetch?.Invoke(currentPage, sets);

                    getSetsRequest = null;
                    currentPage++;
                };

                API.Queue(getSetsRequest);
            }
        }

        public void Reset()
        {
            IsLastPageFetched = false;

            currentPage = 1;

            getSetsRequest?.Cancel();
            getSetsRequest = null;
        }

        public delegate void PageFetchHandler(int page, List<BeatmapSetInfo> sets);
    }
}
