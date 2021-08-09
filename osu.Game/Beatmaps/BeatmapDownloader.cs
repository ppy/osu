// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using static osu.Game.Overlays.BeatmapListing.BeatmapListingFilterControl;

namespace osu.Game.Beatmaps
{
    public class BeatmapDownloader
    {
        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }
        private BeatmapManager beatmapManager { get; set; }
        private IAPIProvider api { get; set; }
        private RulesetStore rulesets { get; set; }
        private Bindable<DateTime> lastBeatmapDownloadTime { get; set; }
        private Bindable<double> minimumStarRating { get; set; }
        private Bindable<bool> noVideoSetting { get; set; }
        private Bindable<SearchCategory> category { get; set; }
        private Bindable<int> ruelsetId { get; set; }

        private const int max_requests = 10;
        private bool finished = false;
        private bool forceStop = false;
        private readonly object downloadLock = new object();

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
            category = config.GetBindable<SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory);
            ruelsetId = config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset);

            if (config.GetBindable<bool>(OsuSetting.BeatmapDownloadStartUp).Value)
            {
                Task.Run(DownloadBeatmaps);
            }
        }

        /// <summary>
        /// Sends the <see cref="SearchBeatmapSetsRequest"/> to the API and processes the Result.
        /// </summary>
        /// <returns>Whether it was successful or not.</returns>
        public Task<bool> DownloadBeatmaps()
        {
            return Task.Run(() =>
            {
                lock (downloadLock)
                {
                    try
                    {
                        RulesetInfo ruleset = rulesets.GetRuleset(ruelsetId.Value);
                        SearchCategory searchCategory = category.Value;

                        if (!api.IsLoggedIn)
                        {
                            throw new DownloaderException(@"You need to be logged in to download Beatmaps");
                        }

                        if (ruleset == null)
                        {
                            throw new DownloaderException(@"No Ruleset found with this ID");
                        }

                        if (lastBeatmapDownloadTime.Value >= DateTime.Now)
                        {
                            lastBeatmapDownloadTime.Value = DateTime.Now;
                            throw new DownloaderException(@"lastBeatmapDownloadTime was higher than DateTime.Now");
                        }

                        if (lastBeatmapDownloadTime.Value > DateTime.Now.AddMinutes(-1))
                        {
                            throw new DownloaderException(@"Please wait a Minute before requesting new Beatmaps");
                        }

                        finished = false;

                        downloadIteration(0, ruleset, searchCategory, null);

                        while (!finished) { Task.Delay(100); }

                        notifications?.Post(new SimpleNotification
                        {
                            Text = $"Finished downloading new Beatmaps",
                            Icon = FontAwesome.Solid.Cross,
                        });

                        return true;
                    }
                    catch (DownloaderException ex)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = $"An Error has occured while downloading the Beatmaps: {ex.Message}",
                            Icon = FontAwesome.Solid.Cross,
                        });
                        return false;
                    }
                    catch (Exception ex)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = $"An internal Error has occured while downloading the Beatmaps: {ex.Message}",
                            Icon = FontAwesome.Solid.Cross,
                        });
                        return false;
                    }

                }
            });
        }

        /// <summary>
        /// Stops the Downloader in the next Iteration if it's running.
        /// </summary>
        public void ForceStop()
        {
            if (!finished)
            {
                forceStop = true;
            }
        }

        /// <summary>
        /// A recursive Function that downloads <see cref="BeatmapSetInfo"/> until certain Criterias are met.
        /// </summary>
        /// <param name="iteration">Number of Iterations beforehand.</param>
        /// <param name="ruleset">A <see cref="RulesetInfo"/> that filters the APIRequest.</param>
        /// <param name="searchCategory">A <see cref="SearchCategory"/> that filters the APIRequest.</param>
        /// <param name="cursor">A <see cref="Cursor"/> from the last <see cref="SearchBeatmapSetsResponse"/>, default is null.</param>
        private void downloadIteration(int iteration, RulesetInfo ruleset, SearchCategory searchCategory, Cursor cursor = null)
        {
            SearchBeatmapSetsRequest getSetsRequest = new SearchBeatmapSetsRequest($"star>={minimumStarRating.Value}", ruleset, cursor, null, searchCategory);

            getSetsRequest.Success += (response) =>
            {
                var sets = response.BeatmapSets.Select(responseJson => responseJson.ToBeatmapSet(rulesets)).ToList();

                if (!handleSearchResults(SearchResult.ResultsReturned(sets)) && sets.Count > 0 && iteration < max_requests && !forceStop)
                {
                    downloadIteration(++iteration, ruleset, searchCategory, response.Cursor);
                }
                else
                {
                    lastBeatmapDownloadTime.Value = DateTime.Now;
                    finished = true;
                }
            };

            api.Queue(getSetsRequest);
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

    [Serializable]
    internal class DownloaderException : Exception
    {

        public DownloaderException(string message) : base(message)
        {
        }
    }
}
