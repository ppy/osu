// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using static osu.Game.Overlays.BeatmapListing.BeatmapListingFilterControl;

namespace osu.Game.Beatmaps
{
    public class BeatmapDownloader
    {
        private BeatmapManager beatmapManager { get; set; }
        private IAPIProvider api { get; set; }
        private RulesetStore rulesets { get; set; }
        private Bindable<DateTime> lastBeatmapDownloadTime { get; set; }
        private Bindable<double> minimumStarRating { get; set; }
        private Bindable<bool> noVideoSetting { get; set; }
        private Bindable<Overlays.BeatmapListing.SearchCategory> category { get; set; }
        private Bindable<int> ruelsetId { get; set; }

        private const int max_requests = 10;

        /// <summary> 
        /// Downloads Maps that have a higher MaxStarDifficulty than specified in OsuSetting.BeatmapDownloadMinimumStarRating
        /// It stops downloading when it hits a Map that is ranked earlier then OsuSetting.BeatmapDownloadLastTime
        /// </summary>
        public BeatmapDownloader(OsuConfigManager config, BeatmapManager beatmapManager, IAPIProvider api, RulesetStore rulesets)
        {
            this.beatmapManager = beatmapManager;
            this.api = api;
            this.rulesets = rulesets;

            noVideoSetting = config.GetBindable<bool>(OsuSetting.PreferNoVideo);
            lastBeatmapDownloadTime = config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime);
            minimumStarRating = config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating);
            category = config.GetBindable<Overlays.BeatmapListing.SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory);
            ruelsetId = config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset);
        }

        /// <summary>
        /// Sends the SearchBeatmapSetsRequest to the API and process the Result
        /// </summary>
        /// <returns>If it was successful or not</returns>
        public Task<bool> DownloadBeatmapsAsync()
        {
            return Task.Run(() =>
            {
                //needed if we need to do multiple api reqeusts
                bool upToDate = false;
                int nReqeusts = 0;
                SearchBeatmapSetsRequest getSetsRequest;
                SearchBeatmapSetsResponse lastResponse = null;

                RulesetInfo ruleset = rulesets.GetRuleset(ruelsetId.Value);
                Overlays.BeatmapListing.SearchCategory searchCategory = category.Value;

                if (ruleset == null || lastBeatmapDownloadTime.Value >= DateTime.Now)
                {
                    return false;
                }

                while (!upToDate)
                {
                    bool currRunFinished = false;

                    getSetsRequest = new SearchBeatmapSetsRequest(@"", ruleset, lastResponse?.Cursor, null, searchCategory);

                    getSetsRequest.Success += response =>
                                {
                                    var sets = response.BeatmapSets.Select(responseJson => responseJson.ToBeatmapSet(rulesets)).ToList();

                                    lastResponse = response;
                                    getSetsRequest = null;

                                    if (sets.Count == 0 || nReqeusts >= max_requests)
                                    {
                                        upToDate = true;
                                        return;
                                    }

                                    if (handleSearchResults(SearchResult.ResultsReturned(sets)))
                                    {
                                        upToDate = true;
                                    }

                                    currRunFinished = true;
                                };

                    api.Queue(getSetsRequest);
                    nReqeusts++;

                    while (!currRunFinished) { Task.Delay(100); }
                }

                lastBeatmapDownloadTime.Value = DateTime.Now;

                return true;
            });
        }

        /// <summary>
        /// Filters and downloads any Maps with the matching Criteria
        /// </summary>
        /// <param name="result">Results from a SearchBeatmapSetsRequest</param>
        /// <returns>Whether it found a Map that was ranked/released earlier than lastBeatmapDownloadTime</returns>
        private bool handleSearchResults(SearchResult result)
        {
            bool hitLastOne = false;

            foreach (var beatmapSetInfo in result.Results)
            {
                switch (beatmapSetInfo.OnlineInfo.Status)
                {
                    case BeatmapSetOnlineStatus.None:
                        break;
                    case BeatmapSetOnlineStatus.Graveyard:
                    case BeatmapSetOnlineStatus.WIP:
                    case BeatmapSetOnlineStatus.Pending:
                        if (beatmapSetInfo.DateAdded.DateTime > lastBeatmapDownloadTime.Value)
                        {
                            if (beatmapSetInfo.MaxStarDifficulty >= minimumStarRating.Value)
                            {
                                beatmapManager.Download(beatmapSetInfo, noVideoSetting.Value);
                            }
                        }
                        else
                        {
                            hitLastOne = true;
                        }
                        break;
                    case BeatmapSetOnlineStatus.Ranked:
                    case BeatmapSetOnlineStatus.Approved:
                    case BeatmapSetOnlineStatus.Qualified:
                    case BeatmapSetOnlineStatus.Loved:
                        if (beatmapSetInfo.OnlineInfo.Ranked?.DateTime > lastBeatmapDownloadTime.Value)
                        {
                            if (beatmapSetInfo.MaxStarDifficulty >= minimumStarRating.Value)
                            {
                                beatmapManager.Download(beatmapSetInfo, noVideoSetting.Value);
                            }
                        }
                        else
                        {
                            hitLastOne = true;
                        }
                        break;
                }
            }

            return hitLastOne;
        }
    }
}
