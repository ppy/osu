// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Localisation;
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
        protected BeatmapManager beatmapManager { get; set; }
        protected IAPIProvider api { get; set; }
        protected RulesetStore rulesets { get; set; }
        private Bindable<DateTime> lastBeatmapDownloadTime { get; set; }
        private Bindable<double> minimumStarRating { get; set; }
        private Bindable<bool> noVideoSetting { get; set; }
        private Bindable<SearchCategory> category { get; set; }
        private Bindable<int> ruelsetId { get; set; }

        private const int max_requests = 10;
        protected bool finished = false;
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
        /// <returns>The Error Message if there is any, otherwise <see cref="string.Empty"/></returns>
        public Task<string> DownloadBeatmaps()
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
                            throw new DownloaderException(BeatmapDownloaderStrings.YouNeedToBeLoggedInToDownloadBeatmaps.ToString());
                        }

                        if (ruleset == null)
                        {
                            throw new DownloaderException(BeatmapDownloaderStrings.NoRulesetFoundWithThisID.ToString());
                        }

                        if (lastBeatmapDownloadTime.Value >= DateTime.Now)
                        {
                            lastBeatmapDownloadTime.Value = DateTime.Now;
                            throw new DownloaderException(BeatmapDownloaderStrings.TheLastDownloadedBeatmapTimeIsInTheFuture.ToString());
                        }

                        if (lastBeatmapDownloadTime.Value > DateTime.Now.AddMinutes(-1))
                        {
                            throw new DownloaderException(BeatmapDownloaderStrings.PleaseWaitAMinuteBeforeRequestingNewBeatmaps.ToString());
                        }

                        finished = false;

                        sendAPIReqeust(0, ruleset, searchCategory, null);

                        while (!finished) { Task.Delay(100); }

                        notifications?.Post(new SimpleNotification
                        {
                            Text = BeatmapDownloaderStrings.FinishedDownloadingNewBeatmaps.ToString(),
                            Icon = FontAwesome.Solid.Check,
                        });

                        return string.Empty;
                    }
                    catch (DownloaderException ex)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = BeatmapDownloaderStrings.AnErrorHasOccuredWhileDownloadingTheBeatmaps(ex.Message).ToString(),
                            Icon = FontAwesome.Solid.Cross,
                        });
                        return ex.Message;
                    }
                    catch (Exception ex)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = BeatmapDownloaderStrings.AnInternalErrorHasOccuredWhileDownloadingTheBeatmaps(ex.Message).ToString(),
                            Icon = FontAwesome.Solid.Cross,
                        });
                        return ex.Message;
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
        /// Sends an APIReqeust that recursively downloads <see cref="BeatmapSetInfo"/> until certain Criterias are met.
        /// </summary>
        /// <param name="iteration">Number of Iterations beforehand.</param>
        /// <param name="ruleset">A <see cref="RulesetInfo"/> that filters the APIRequest.</param>
        /// <param name="searchCategory">A <see cref="SearchCategory"/> that filters the APIRequest.</param>
        /// <param name="cursor">A <see cref="Cursor"/> from the last <see cref="SearchBeatmapSetsResponse"/>, default is null.</param>
        protected virtual void sendAPIReqeust(int iteration, RulesetInfo ruleset, SearchCategory searchCategory, Cursor cursor = null)
        {
            SearchBeatmapSetsRequest getSetsRequest = new SearchBeatmapSetsRequest($"star>={minimumStarRating.Value}", ruleset, cursor, null, searchCategory);

            getSetsRequest.Success += (response) =>
            handleAPIReqeust(SearchResult.ResultsReturned(response.BeatmapSets.Select(responseJson => responseJson.ToBeatmapSet(rulesets)).ToList()),
                                cursor, iteration, ruleset, searchCategory);

            api.Queue(getSetsRequest);
        }

        /// <summary>
        /// Handles the APIRequest from <see cref="sendAPIReqeust(int, RulesetInfo, SearchCategory, Cursor)"/>.
        /// </summary>
        /// <param name="result">A <see cref="SearchResult"/> from the last <see cref="SearchBeatmapSetsResponse"/>.</param>
        /// <param name="cursor">A <see cref="Cursor"/> from the last <see cref="SearchBeatmapSetsResponse"/>.</param>
        /// <param name="iteration">Number of Iterations beforehand.</param>
        /// <param name="ruleset">A <see cref="RulesetInfo"/> that filters the APIRequest.</param>
        /// <param name="searchCategory">A <see cref="SearchCategory"/> that filters the APIRequest.</param>
        protected void handleAPIReqeust(SearchResult result, Cursor cursor, int iteration, RulesetInfo ruleset, SearchCategory searchCategory)
        {
            if (!handleSearchResults(result) && result.Results.Count > 0 && iteration < max_requests && !forceStop)
            {
                sendAPIReqeust(++iteration, ruleset, searchCategory, cursor);
            }
            else
            {
                lastBeatmapDownloadTime.Value = DateTime.Now;
                finished = true;
            }
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
                    downloadBeatmap(beatmapSetInfo);
                }
            }

            return result.Results.Where(b => !isAfterLastBeatmapDownloadTime(b)).Any();
        }

        /// <summary>
        /// Downloads and imports the BeatmapSet
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to download</param>
        protected virtual void downloadBeatmap(BeatmapSetInfo beatmapSetInfo)
        {
            beatmapManager.Download(beatmapSetInfo, noVideoSetting.Value);
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
