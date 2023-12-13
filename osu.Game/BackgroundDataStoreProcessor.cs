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

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets which require reprocessing.");

            int i = 0;

            foreach (var id in beatmapSetIds)
            {
                sleepIfRequired();

                realmAccess.Run(r =>
                {
                    var set = r.Find<BeatmapSetInfo>(id);

                    if (set != null)
                    {
                        try
                        {
                            Logger.Log($"Background processing {set} ({++i} / {beatmapSetIds.Count})");
                            beatmapUpdater.Process(set);
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Background processing failed on {set}: {e}");
                        }
                    }
                });
            }
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

            Logger.Log($"Found {beatmapIds.Count} beatmaps which require reprocessing.");

            int i = 0;

            foreach (var id in beatmapIds)
            {
                sleepIfRequired();

                realmAccess.Run(r =>
                {
                    var beatmap = r.Find<BeatmapInfo>(id);

                    if (beatmap != null)
                    {
                        try
                        {
                            Logger.Log($"Background processing {beatmap} ({++i} / {beatmapIds.Count})");
                            beatmapUpdater.ProcessObjectCounts(beatmap);
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Background processing failed on {beatmap}: {e}");
                        }
                    }
                });
            }
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

            Logger.Log($"Found {scoreIds.Count} scores which require reprocessing.");

            foreach (var id in scoreIds)
            {
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
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Log(@$"Failed to populate maximum statistics for {id}: {e}");
                    realmAccess.Write(r => r.Find<ScoreInfo>(id)!.BackgroundReprocessingFailed = true);
                }
            }
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

            ProgressNotification notification = new ProgressNotification { State = ProgressNotificationState.Active };

            notificationOverlay?.Post(notification);

            int processedCount = 0;
            int failedCount = 0;

            foreach (var id in scoreIds)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    break;

                notification.Text = $"Upgrading scores to new scoring algorithm ({processedCount} of {scoreIds.Count})";
                notification.Progress = (float)processedCount / scoreIds.Count;

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

            if (processedCount == scoreIds.Count)
            {
                notification.CompletionText = $"{processedCount} score(s) have been upgraded to the new scoring algorithm";
                notification.Progress = 1;
                notification.State = ProgressNotificationState.Completed;
            }
            else
            {
                notification.Text = $"{processedCount} of {scoreIds.Count} score(s) have been upgraded to the new scoring algorithm.";

                // We may have arrived here due to user cancellation or completion with failures.
                if (failedCount > 0)
                    notification.Text += $" Check logs for issues with {failedCount} failed upgrades.";

                notification.State = ProgressNotificationState.Cancelled;
            }
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
