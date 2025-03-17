// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Performance;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;

namespace osu.Game.Database
{
    /// <summary>
    /// Performs background updating of data stores at startup.
    /// </summary>
    public partial class BackgroundDataStoreProcessor : Component
    {
        protected Task ProcessingTask { get; private set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private IBeatmapUpdater beatmapUpdater { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserPlayInfo { get; set; }

        [Resolved]
        private IHighPerformanceSessionManager? highPerformanceSessionManager { get; set; }

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private Storage storage { get; set; } = null!;

        protected virtual int TimeToSleepDuringGameplay => 30000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ProcessingTask = Task.Factory.StartNew(() =>
            {
                Logger.Log("Beginning background data store processing..");

                clearOutdatedStarRatings();
                populateMissingStarRatings();
                processOnlineBeatmapSetsWithNoUpdate();
                // Note that the previous method will also update these on a fresh run.
                processBeatmapsWithMissingObjectCounts();
                processScoresWithMissingStatistics();
                convertLegacyTotalScoreToStandardised();
                upgradeScoreRanks();
                backpopulateMissingSubmissionAndRankDates();
            }, TaskCreationOptions.LongRunning).ContinueWith(t =>
            {
                if (t.Exception?.InnerException is ObjectDisposedException)
                {
                    Logger.Log("Finished background aborted during shutdown");
                    return;
                }

                Logger.Log("Finished background data store processing!");
            });
        }

        /// <summary>
        /// Check whether the databased difficulty calculation version matches the latest ruleset provided version.
        /// If it doesn't, clear out any existing difficulties so they can be incrementally recalculated.
        /// </summary>
        private void clearOutdatedStarRatings()
        {
            foreach (var ruleset in rulesetStore.AvailableRulesets)
            {
                // beatmap being passed in is arbitrary here. just needs to be non-null.
                int currentVersion = ruleset.CreateInstance().CreateDifficultyCalculator(gameBeatmap.Value).Version;

                if (ruleset.LastAppliedDifficultyVersion < currentVersion)
                {
                    Logger.Log($"Resetting star ratings for {ruleset.Name} (difficulty calculation version updated from {ruleset.LastAppliedDifficultyVersion} to {currentVersion})");

                    int countReset = 0;

                    realmAccess.Write(r =>
                    {
                        foreach (var b in r.All<BeatmapInfo>())
                        {
                            if (b.Ruleset.ShortName == ruleset.ShortName)
                            {
                                b.StarRating = -1;
                                countReset++;
                            }
                        }

                        r.Find<RulesetInfo>(ruleset.ShortName)!.LastAppliedDifficultyVersion = currentVersion;
                    });

                    Logger.Log($"Finished resetting {countReset} beatmap sets for {ruleset.Name}");
                }
            }
        }

        /// <remarks>
        /// This is split out from <see cref="processOnlineBeatmapSetsWithNoUpdate"/> as a separate process to prevent high server-side load
        /// from the <see cref="beatmapUpdater"/> firing online requests as part of the update.
        /// Star rating recalculations can be ran strictly locally.
        /// </remarks>
        private void populateMissingStarRatings()
        {
            HashSet<Guid> beatmapIds = new HashSet<Guid>();

            Logger.Log("Querying for beatmaps with missing star ratings...");

            realmAccess.Run(r =>
            {
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.StarRating < 0 && b.BeatmapSet != null))
                    beatmapIds.Add(b.ID);
            });

            if (beatmapIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapIds.Count} beatmaps which require star rating reprocessing.");

            var notification = showProgressNotification(beatmapIds.Count, "Reprocessing star rating for beatmaps", "beatmaps' star ratings have been updated");

            int processedCount = 0;
            int failedCount = 0;

            Dictionary<string, Ruleset> rulesetCache = new Dictionary<string, Ruleset>();

            Ruleset getRuleset(RulesetInfo rulesetInfo)
            {
                if (!rulesetCache.TryGetValue(rulesetInfo.ShortName, out var ruleset))
                    ruleset = rulesetCache[rulesetInfo.ShortName] = rulesetInfo.CreateInstance();

                return ruleset;
            }

            foreach (Guid id in beatmapIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, beatmapIds.Count);

                sleepIfRequired();

                var beatmap = realmAccess.Run(r => r.Find<BeatmapInfo>(id)?.Detach());

                if (beatmap == null)
                    return;

                try
                {
                    var working = beatmapManager.GetWorkingBeatmap(beatmap);
                    var ruleset = getRuleset(working.BeatmapInfo.Ruleset);

                    Debug.Assert(ruleset != null);

                    var calculator = ruleset.CreateDifficultyCalculator(working);

                    double starRating = calculator.Calculate().StarRating;
                    realmAccess.Write(r =>
                    {
                        if (r.Find<BeatmapInfo>(id) is BeatmapInfo liveBeatmapInfo)
                            liveBeatmapInfo.StarRating = starRating;
                    });
                    ((IWorkingBeatmapCache)beatmapManager).Invalidate(beatmap);
                    ++processedCount;
                }
                catch (Exception e)
                {
                    Logger.Log($"Background processing failed on {beatmap}: {e}");
                    ++failedCount;
                }
            }

            completeNotification(notification, processedCount, beatmapIds.Count, failedCount);
        }

        private void processOnlineBeatmapSetsWithNoUpdate()
        {
            HashSet<Guid> beatmapSetIds = new HashSet<Guid>();

            Logger.Log("Querying for beatmap sets to reprocess...");

            realmAccess.Run(r =>
            {
                // BeatmapProcessor is responsible for both online and local processing.
                // In the case a user isn't logged in, it won't update LastOnlineUpdate and therefore re-queue,
                // causing overhead from the non-online processing to redundantly run every startup.
                //
                // We may eventually consider making the Process call more specific (or avoid this in any number
                // of other possible ways), but for now avoid queueing if the user isn't logged in at startup.
                if (api.IsLoggedIn)
                {
                    foreach (var b in r.All<BeatmapInfo>().Where(b => b.OnlineID > 0 && b.LastOnlineUpdate == null && b.BeatmapSet != null))
                        beatmapSetIds.Add(b.BeatmapSet!.ID);
                }
            });

            if (beatmapSetIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets which require online updates.");

            var notification = showProgressNotification(beatmapSetIds.Count, "Updating online data for beatmaps", "beatmaps' online data have been updated");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in beatmapSetIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, beatmapSetIds.Count);

                sleepIfRequired();

                realmAccess.Run(r =>
                {
                    var set = r.Find<BeatmapSetInfo>(id);

                    if (set != null)
                    {
                        try
                        {
                            beatmapUpdater.Process(set);
                            ++processedCount;
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Background processing failed on {set}: {e}");
                            ++failedCount;
                        }
                    }
                });
            }

            completeNotification(notification, processedCount, beatmapSetIds.Count, failedCount);
        }

        private void processBeatmapsWithMissingObjectCounts()
        {
            Logger.Log("Querying for beatmaps with missing hitobject counts to reprocess...");

            HashSet<Guid> beatmapIds = new HashSet<Guid>();

            realmAccess.Run(r =>
            {
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.TotalObjectCount < 0 || b.EndTimeObjectCount < 0))
                    beatmapIds.Add(b.ID);
            });

            if (beatmapIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapIds.Count} beatmaps which require statistics population.");

            var notification = showProgressNotification(beatmapIds.Count, "Populating missing statistics for beatmaps", "beatmaps have been populated with missing statistics");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in beatmapIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, beatmapIds.Count);

                sleepIfRequired();

                realmAccess.Run(r =>
                {
                    var beatmap = r.Find<BeatmapInfo>(id);

                    if (beatmap != null)
                    {
                        try
                        {
                            beatmapUpdater.ProcessObjectCounts(beatmap);
                            ++processedCount;
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Background processing failed on {beatmap}: {e}");
                            ++failedCount;
                        }
                    }
                });
            }

            completeNotification(notification, processedCount, beatmapIds.Count, failedCount);
        }

        private void processScoresWithMissingStatistics()
        {
            HashSet<Guid> scoreIds = new HashSet<Guid>();

            Logger.Log("Querying for scores to reprocess...");

            realmAccess.Run(r =>
            {
                foreach (var score in r.All<ScoreInfo>().Where(s => !s.BackgroundReprocessingFailed))
                {
                    if (score.BeatmapInfo != null
                        && score.Statistics.Sum(kvp => kvp.Value) > 0
                        && score.MaximumStatistics.Sum(kvp => kvp.Value) == 0)
                    {
                        scoreIds.Add(score.ID);
                    }
                }
            });

            if (scoreIds.Count == 0)
                return;

            Logger.Log($"Found {scoreIds.Count} scores which require statistics population.");

            var notification = showProgressNotification(scoreIds.Count, "Populating missing statistics for scores", "scores have been populated with missing statistics");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in scoreIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, scoreIds.Count);

                sleepIfRequired();

                try
                {
                    var score = scoreManager.Query(s => s.ID == id);

                    if (score != null)
                    {
                        scoreManager.PopulateMaximumStatistics(score);

                        // Can't use async overload because we're not on the update thread.
                        // ReSharper disable once MethodHasAsyncOverload
                        realmAccess.Write(r =>
                        {
                            r.Find<ScoreInfo>(id)!.MaximumStatisticsJson = JsonConvert.SerializeObject(score.MaximumStatistics);
                        });
                    }

                    ++processedCount;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Log(@$"Failed to populate maximum statistics for {id}: {e}");
                    realmAccess.Write(r => r.Find<ScoreInfo>(id)!.BackgroundReprocessingFailed = true);
                    ++failedCount;
                }
            }

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);
        }

        private void convertLegacyTotalScoreToStandardised()
        {
            Logger.Log("Querying for scores that need total score conversion...");

            HashSet<Guid> scoreIds = realmAccess.Run(r => new HashSet<Guid>(
                r.All<ScoreInfo>()
                 .Where(s => !s.BackgroundReprocessingFailed
                             && s.BeatmapInfo != null
                             && s.IsLegacyScore
                             && s.TotalScoreVersion < LegacyScoreEncoder.LATEST_VERSION)
                 .AsEnumerable()
                 // must be done after materialisation, as realm doesn't want to support
                 // nested property predicates
                 .Where(s => s.Ruleset.IsLegacyRuleset())
                 .Select(s => s.ID)));

            Logger.Log($"Found {scoreIds.Count} scores which require total score conversion.");

            if (scoreIds.Count == 0)
                return;

            var notification = showProgressNotification(scoreIds.Count, "Upgrading scores to new scoring algorithm", "scores have been upgraded to the new scoring algorithm");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in scoreIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, scoreIds.Count);

                sleepIfRequired();

                try
                {
                    // Can't use async overload because we're not on the update thread.
                    // ReSharper disable once MethodHasAsyncOverload
                    realmAccess.Write(r =>
                    {
                        ScoreInfo s = r.Find<ScoreInfo>(id)!;
                        StandardisedScoreMigrationTools.UpdateFromLegacy(s, beatmapManager.GetWorkingBeatmap(s.BeatmapInfo));
                        s.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    });

                    ++processedCount;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to convert total score for {id}: {e}");
                    realmAccess.Write(r => r.Find<ScoreInfo>(id)!.BackgroundReprocessingFailed = true);
                    ++failedCount;
                }
            }

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);
        }

        private void upgradeScoreRanks()
        {
            Logger.Log("Querying for scores that need rank upgrades...");

            HashSet<Guid> scoreIds = realmAccess.Run(r => new HashSet<Guid>(
                r.All<ScoreInfo>()
                 .Where(s => s.TotalScoreVersion < 30000013 && !s.BackgroundReprocessingFailed) // last total score version with a significant change to ranks
                 .AsEnumerable()
                 // must be done after materialisation, as realm doesn't support
                 // filtering on nested property predicates or projection via `.Select()`
                 .Where(s => s.Ruleset.IsLegacyRuleset())
                 .Select(s => s.ID)));

            Logger.Log($"Found {scoreIds.Count} scores which require rank upgrades.");

            if (scoreIds.Count == 0)
                return;

            var notification = showProgressNotification(scoreIds.Count, "Adjusting ranks of scores", "scores now have more correct ranks");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in scoreIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, scoreIds.Count);

                sleepIfRequired();

                try
                {
                    // Can't use async overload because we're not on the update thread.
                    // ReSharper disable once MethodHasAsyncOverload
                    realmAccess.Write(r =>
                    {
                        ScoreInfo s = r.Find<ScoreInfo>(id)!;
                        s.Rank = StandardisedScoreMigrationTools.ComputeRank(s);
                        s.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    });

                    ++processedCount;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to update rank score {id}: {e}");
                    realmAccess.Write(r => r.Find<ScoreInfo>(id)!.BackgroundReprocessingFailed = true);
                    ++failedCount;
                }
            }

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);
        }

        private void backpopulateMissingSubmissionAndRankDates()
        {
            var localMetadataSource = new LocalCachedBeatmapMetadataSource(storage);

            if (!localMetadataSource.Available)
            {
                Logger.Log("Cannot backpopulate missing submission/rank dates because the local metadata cache is missing.");
                return;
            }

            try
            {
                if (localMetadataSource.GetCacheVersion() < 2)
                {
                    Logger.Log("Cannot backpopulate missing submission/rank dates because the local metadata cache is too old.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error when trying to query version of local metadata cache: {ex}");
                return;
            }

            Logger.Log("Querying for beatmap sets that contain missing submission/rank date...");

            HashSet<Guid> beatmapSetIds = realmAccess.Run(r => new HashSet<Guid>(
                r.All<BeatmapSetInfo>()
                 .Where(b => b.StatusInt > 0 && (b.DateRanked == null || b.DateSubmitted == null))
                 .AsEnumerable()
                 .Select(b => b.ID)));

            if (beatmapSetIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets with missing submission/rank date.");

            var notification = showProgressNotification(beatmapSetIds.Count, "Populating missing submission and rank dates", "beatmap sets now have correct submission and rank dates.");

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in beatmapSetIds)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                updateNotificationProgress(notification, processedCount, beatmapSetIds.Count);

                sleepIfRequired();

                try
                {
                    // Can't use async overload because we're not on the update thread.
                    // ReSharper disable once MethodHasAsyncOverload
                    bool succeeded = realmAccess.Write(r =>
                    {
                        BeatmapSetInfo beatmapSet = r.Find<BeatmapSetInfo>(id)!;

                        // we want any ranked representative of the set.
                        // the reason for checking ranked status of the difficulty is that it can be locally modified,
                        // at which point the lookup will fail - but there might still be another unmodified difficulty on which it will work.
                        if (beatmapSet.Beatmaps.FirstOrDefault(b => b.Status >= BeatmapOnlineStatus.Ranked) is not BeatmapInfo beatmap)
                            return false;

                        bool lookupSucceeded = localMetadataSource.TryLookup(beatmap, out var result);

                        if (lookupSucceeded)
                        {
                            Debug.Assert(result != null);
                            beatmapSet.DateRanked = result.DateRanked;
                            beatmapSet.DateSubmitted = result.DateSubmitted;
                            return true;
                        }

                        Logger.Log($"Could not find {beatmapSet.GetDisplayString()} in local cache while backpopulating missing submission/rank date");
                        return false;
                    });

                    if (succeeded)
                        ++processedCount;
                    else
                        ++failedCount;
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to update ranked/submitted dates for beatmap set {id}: {e}");
                    ++failedCount;
                }
            }

            completeNotification(notification, processedCount, beatmapSetIds.Count, failedCount);
        }

        private void updateNotificationProgress(ProgressNotification? notification, int processedCount, int totalCount)
        {
            if (notification == null)
                return;

            notification.Text = notification.Text.ToString().Split('(').First().TrimEnd() + $" ({processedCount} of {totalCount})";
            notification.Progress = (float)processedCount / totalCount;

            if (processedCount % 100 == 0)
                Logger.Log(notification.Text.ToString());
        }

        private void completeNotification(ProgressNotification? notification, int processedCount, int totalCount, int? failedCount = null)
        {
            if (notification == null)
                return;

            if (processedCount == totalCount)
            {
                notification.CompletionText = $"{processedCount} {notification.CompletionText}";
                notification.Progress = 1;
                notification.State = ProgressNotificationState.Completed;
            }
            else
            {
                notification.Text = $"{processedCount} of {totalCount} {notification.CompletionText}";

                // We may have arrived here due to user cancellation or completion with failures.
                if (failedCount > 0)
                    notification.Text += $" Check logs for issues with {failedCount} failed items.";

                notification.State = ProgressNotificationState.Cancelled;
            }
        }

        private ProgressNotification? showProgressNotification(int totalCount, string running, string completed)
        {
            if (notificationOverlay == null)
                return null;

            if (totalCount < 10)
                return null;

            ProgressNotification notification = new ProgressNotification
            {
                Text = running,
                CompletionText = completed,
                State = ProgressNotificationState.Active
            };

            notificationOverlay?.Post(notification);

            return notification;
        }

        private void sleepIfRequired()
        {
            // Importantly, also sleep if high performance session is active.
            // If we don't do this, memory usage can become runaway due to GC running in a more lenient mode.
            while (localUserPlayInfo?.PlayingState.Value != LocalUserPlayingState.NotPlaying || highPerformanceSessionManager?.IsSessionActive == true)
            {
                Logger.Log("Background processing sleeping due to active gameplay...");
                Thread.Sleep(TimeToSleepDuringGameplay);
            }
        }
    }
}
