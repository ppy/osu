// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
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

        private const int max_requests = 100;

        /// <summary> 
        /// Downloads <see cref="BeatmapSetInfo"/> that have a higher <see cref="BeatmapSetInfo.MaxStarDifficulty"/>  than specified in <see cref="OsuSetting.BeatmapDownloadMinimumStarRating"/>.
        /// It stops requesting more <see cref="BeatmapSetInfo"/> when it gets a BeatmapSet that is uploaded/ranked earlier then <see cref="OsuSetting.BeatmapDownloadLastTime"/>.
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
        /// Sends the <see cref="SearchBeatmapSetsRequest"/> to the API and processes the Result.
        /// </summary>
        /// <returns>If it was successful or not.</returns>
        public Task<bool> DownloadBeatmapsAsync()
        {
            return Task.Run(() =>
            {
                //needed if multiple api reqeusts are required
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

                    getSetsRequest = new SearchBeatmapSetsRequest($"star>={minimumStarRating.Value}", ruleset, lastResponse?.Cursor, null, searchCategory);

                    getSetsRequest.Success += response =>
                                {
                                    var sets = response.BeatmapSets.Select(responseJson => responseJson.ToBeatmapSet(rulesets)).ToList();

                                    lastResponse = response;
                                    getSetsRequest = null;

                                    if (sets.Count == 0)
                                    {
                                        upToDate = true;
                                        return;
                                    }

                                    if (handleSearchResults(SearchResult.ResultsReturned(sets)) || nReqeusts >= max_requests)
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
        /// Filters and downloads any <see cref="BeatmapSetInfo"/> with the matching Criteria.
        /// </summary>
        /// <param name="result"><see cref="SearchResult"/> from a <see cref="SearchBeatmapSetsRequest"/>.</param>
        /// <returns>Whether it found a <see cref="BeatmapSetInfo"/> that was uploaded/ranked before the <see cref="OsuSetting.BeatmapDownloadLastTime"/> Date.</returns>
        private bool handleSearchResults(SearchResult result)
        {
            foreach (var beatmapSetInfo in result.Results)
            {
                if (MatchesDownloadCriteria(beatmapSetInfo))
                {
                    beatmapManager.Download(beatmapSetInfo, noVideoSetting.Value);
                }
            }

            return result.Results.Where(b => !isAfterLastBeatmapDownloadTime(b)).Any();
        }

        /// <summary>
        /// Checks whether the <see cref="BeatmapSetInfo"/> matches the Download Criteria.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to check.</param>
        /// <returns>Whether the <see cref="BeatmapSetInfo"/> matches the Criteria.</returns>
        public bool MatchesDownloadCriteria(BeatmapSetInfo beatmapSetInfo)
        {
            //May be expanded at some point
            return isAfterLastBeatmapDownloadTime(beatmapSetInfo);
        }

        /// <summary>
        /// Checks whether the <see cref="BeatmapSetInfo"/> was uploaded/ranked before or after the <see cref="OsuSetting.BeatmapDownloadLastTime"/> Variable.
        /// Uses <see cref="BeatmapSetOnlineInfo.Ranked"/> Date when available otherwise the <see cref="BeatmapSetOnlineInfo.LastUpdated"/> Date.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to check.</param>
        /// <returns>Whether the <see cref="BeatmapSetInfo"/> Date is before or after <see cref="OsuSetting.BeatmapDownloadLastTime"/>.</returns>
        private bool isAfterLastBeatmapDownloadTime(BeatmapSetInfo beatmapSetInfo)
        {
            return (beatmapSetInfo.OnlineInfo.Ranked == null && beatmapSetInfo.OnlineInfo.LastUpdated?.DateTime > lastBeatmapDownloadTime.Value) || beatmapSetInfo.OnlineInfo.Ranked?.DateTime > lastBeatmapDownloadTime.Value;
        }
    }
}
