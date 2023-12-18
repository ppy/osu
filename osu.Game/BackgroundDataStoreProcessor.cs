// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;

namespace osu.Game
{
    /// <summary>
    /// Performs background updating of data stores at startup.
    /// </summary>
    public partial class BackgroundDataStoreProcessor : Component
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private BeatmapUpdater beatmapUpdater { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserPlayInfo { get; set; }

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        protected virtual int TimeToSleepDuringGameplay => 30000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Factory.StartNew(() =>
            {
                Logger.Log("Beginning background data store processing..");

                checkForOutdatedStarRatings();
                processBeatmapSetsWithMissingMetrics();
                processBeatmapsWithMissingObjectCounts();
                processScoresWithMissingStatistics();
                convertLegacyTotalScoreToStandardised();
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
        private void checkForOutdatedStarRatings()
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

        private void processBeatmapSetsWithMissingMetrics()
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
                    foreach (var b in r.All<BeatmapInfo>().Where(b => (b.StarRating < 0 || (b.OnlineID > 0 && b.LastOnlineUpdate == null)) && b.BeatmapSet != null))
                        beatmapSetIds.Add(b.BeatmapSet!.ID);
                }
                else
                {
                    foreach (var b in r.All<BeatmapInfo>().Where(b => b.StarRating < 0 && b.BeatmapSet != null))
                        beatmapSetIds.Add(b.BeatmapSet!.ID);
                }
            });

            if (beatmapSetIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets which require reprocessing.");

            // Technically this is doing more than just star ratings, but easier for the end user to understand.
            var notification = showProgressNotification("Reprocessing star rating for beatmaps", "beatmaps' star ratings have been updated");

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
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.TotalObjectCount == 0))
                    beatmapIds.Add(b.ID);
            });

            if (beatmapIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapIds.Count} beatmaps which require statistics population.");

            var notification = showProgressNotification("Populating missing statistics for beatmaps", "beatmaps have been populated with missing statistics");

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

            var notification = showProgressNotification("Populating missing statistics for scores", "scores have been populated with missing statistics");

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

                    scoreManager.PopulateMaximumStatistics(score);

                    // Can't use async overload because we're not on the update thread.
                    // ReSharper disable once MethodHasAsyncOverload
                    realmAccess.Write(r =>
                    {
                        r.Find<ScoreInfo>(id)!.MaximumStatisticsJson = JsonConvert.SerializeObject(score.MaximumStatistics);
                    });

                    Logger.Log($"Populated maximum statistics for score {id}");
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

            HashSet<Guid> scoreIds = realmAccess.Run(r => new HashSet<Guid>(r.All<ScoreInfo>()
                                                                             .Where(s => !s.BackgroundReprocessingFailed && s.BeatmapInfo != null
                                                                                                                         && (s.TotalScoreVersion == 30000002
                                                                                                                             || s.TotalScoreVersion == 30000003))
                                                                             .AsEnumerable().Select(s => s.ID)));

            Logger.Log($"Found {scoreIds.Count} scores which require total score conversion.");

            if (scoreIds.Count == 0)
                return;

            var notification = showProgressNotification("Upgrading scores to new scoring algorithm", "scores have been upgraded to the new scoring algorithm");

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
                    long newTotalScore = StandardisedScoreMigrationTools.ConvertFromLegacyTotalScore(score, beatmapManager);

                    // Can't use async overload because we're not on the update thread.
                    // ReSharper disable once MethodHasAsyncOverload
                    realmAccess.Write(r =>
                    {
                        ScoreInfo s = r.Find<ScoreInfo>(id)!;
                        s.TotalScore = newTotalScore;
                        s.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    });

                    Logger.Log($"Converted total score for score {id}");
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

        private void updateNotificationProgress(ProgressNotification? notification, int processedCount, int totalCount)
        {
            if (notification == null)
                return;

            notification.Text = notification.Text.ToString().Split('(').First().TrimEnd() + $" ({processedCount} of {totalCount})";
            notification.Progress = (float)processedCount / totalCount;

            // TODO add log output
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

        private ProgressNotification? showProgressNotification(string running, string completed)
        {
            if (notificationOverlay == null)
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
            while (localUserPlayInfo?.IsPlaying.Value == true)
            {
                Logger.Log("Background processing sleeping due to active gameplay...");
                Thread.Sleep(TimeToSleepDuringGameplay);
            }
        }
    }
}
