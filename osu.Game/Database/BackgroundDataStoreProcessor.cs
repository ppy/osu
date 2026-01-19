// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Performance;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;
using Realms;

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

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private LocalCachedBeatmapMetadataSource localMetadataSource = null!;

        protected virtual int TimeToSleepDuringGameplay => 30000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localMetadataSource = new LocalCachedBeatmapMetadataSource(storage);

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
                backpopulateUserTags();
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
                        foreach (var beatmap in r.All<BeatmapInfo>())
                        {
                            if (beatmap.Ruleset.ShortName == ruleset.ShortName)
                            {
                                beatmap.StarRating = -1;
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

            const int chunk_size = 500;
            int processedCount = 0;
            int failedCount = 0;

            Dictionary<string, Ruleset> rulesetCache = new Dictionary<string, Ruleset>();

            Ruleset getRuleset(RulesetInfo rulesetInfo)
            {
                if (!rulesetCache.TryGetValue(rulesetInfo.ShortName, out var ruleset))
                    ruleset = rulesetCache[rulesetInfo.ShortName] = rulesetInfo.CreateInstance();

                return ruleset;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in beatmapIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, beatmapIds.Count);

                    sleepIfRequired();

                    var beatmap = r.Find<BeatmapInfo>(id);

                    if (beatmap == null)
                    {
                        ++processedCount;
                        continue;
                    }

                    try
                    {
                        var working = beatmapManager.GetWorkingBeatmap(beatmap);
                        var ruleset = getRuleset(working.BeatmapInfo.Ruleset);

                        Debug.Assert(ruleset != null);

                        var calculator = ruleset.CreateDifficultyCalculator(working);
                        double starRating = calculator.Calculate().StarRating;

                        actions.Add(realm =>
                        {
                            var item = realm.Find<BeatmapInfo>(id)!;
                            item.StarRating = starRating;

                            ((IWorkingBeatmapCache)beatmapManager).Invalidate(item);
                        });
                        ++processedCount;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Background processing failed on {beatmap}: {e}");
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, beatmapIds.Count, failedCount);

            Logger.Log($"Populating {processedCount} of {beatmapIds.Count} missing star ratings completed in {stopwatch.ElapsedMilliseconds}ms");
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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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

            Logger.Log($"Processing {processedCount} of {beatmapSetIds.Count} online beatmapsets completed in {stopwatch.ElapsedMilliseconds}ms");
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

            const int chunk_size = 500;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in beatmapIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, beatmapIds.Count);

                    sleepIfRequired();

                    var beatmap = r.Find<BeatmapInfo>(id);

                    if (beatmap == null)
                    {
                        ++processedCount;
                        continue;
                    }

                    try
                    {
                        // Moved from BeatmapUpdater.ProcessObjectCounts to reduce writes to disk
                        var workingBeatmapCache = (IWorkingBeatmapCache)beatmapManager;
                        workingBeatmapCache.Invalidate(beatmap);

                        var working = workingBeatmapCache.GetWorkingBeatmap(beatmap);
                        var workingBeatmap = working.Beatmap;

                        int totalObjectCount = workingBeatmap.HitObjects.Count;
                        int endTimeObjectCount = workingBeatmap.HitObjects.Count(h => h is IHasDuration);

                        actions.Add(realm =>
                        {
                            var item = realm.Find<BeatmapInfo>(id)!;

                            item.TotalObjectCount = totalObjectCount;
                            item.EndTimeObjectCount = endTimeObjectCount;

                            ((IWorkingBeatmapCache)beatmapManager).Invalidate(item);
                        });
                        ++processedCount;
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Background processing failed on {beatmap}: {e}");
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, beatmapIds.Count, failedCount);

            Logger.Log($"Processing {processedCount} of {beatmapIds.Count} beatmaps object counts completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private void processScoresWithMissingStatistics()
        {
            Logger.Log("Querying for scores to reprocess...");

            HashSet<Guid> scoreIds = new HashSet<Guid>();

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

            const int chunk_size = 1000;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in scoreIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, scoreIds.Count);

                    sleepIfRequired();

                    var score = r.Find<ScoreInfo>(id)?.Detach();

                    if (score == null)
                    {
                        ++processedCount;
                        continue;
                    }

                    try
                    {
                        scoreManager.PopulateMaximumStatistics(score);
                        string maximumStatisticsJson = JsonConvert.SerializeObject(score.MaximumStatistics);

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;
                            item.MaximumStatisticsJson = maximumStatisticsJson;
                        });
                        ++processedCount;
                    }
                    catch (ObjectDisposedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Logger.Log(@$"Failed to populate maximum statistics for {id}: {e}");

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;
                            item.BackgroundReprocessingFailed = true;
                        });
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);

            Logger.Log($"Processing {processedCount} of {scoreIds.Count} missing scores statistics completed in {stopwatch.ElapsedMilliseconds}ms");
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

            if (scoreIds.Count == 0)
                return;

            Logger.Log($"Found {scoreIds.Count} scores which require total score conversion.");

            var notification = showProgressNotification(scoreIds.Count, "Upgrading scores to new scoring algorithm", "scores have been upgraded to the new scoring algorithm");

            const int chunk_size = 500;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in scoreIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, scoreIds.Count);

                    sleepIfRequired();

                    var score = r.Find<ScoreInfo>(id)?.Detach();

                    if (score == null)
                    {
                        processedCount++;
                        continue;
                    }

                    try
                    {
                        StandardisedScoreMigrationTools.UpdateFromLegacy(score, beatmapManager.GetWorkingBeatmap(score.BeatmapInfo));

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;

                            item.Accuracy = score.Accuracy;
                            item.Rank = score.Rank;
                            item.TotalScore = score.TotalScore;
                            item.TotalScoreWithoutMods = score.TotalScoreWithoutMods;
                            item.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
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

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;
                            item.BackgroundReprocessingFailed = true;
                        });
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);

            Logger.Log($"Converting totalScore {processedCount} of {scoreIds.Count} scores completed in {stopwatch.ElapsedMilliseconds}ms");
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

            var notification = showProgressNotification(scoreIds.Count, "Adjusting ranks of scores", "scores now have more correct ranks.");

            const int chunk_size = 3000;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in scoreIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, scoreIds.Count);

                    sleepIfRequired();

                    var score = r.Find<ScoreInfo>(id);

                    if (score == null)
                    {
                        processedCount++;
                        continue;
                    }

                    try
                    {
                        var rank = StandardisedScoreMigrationTools.ComputeRank(score);

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;

                            item.Rank = rank;
                            item.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
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

                        actions.Add(realm =>
                        {
                            var item = realm.Find<ScoreInfo>(id)!;
                            item.BackgroundReprocessingFailed = true;
                        });
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, scoreIds.Count, failedCount);

            Logger.Log($"Upgrading {processedCount} of {scoreIds.Count} score ranks completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private void backpopulateMissingSubmissionAndRankDates()
        {
            if (!localMetadataSource.Available)
            {
                Logger.Log("Cannot backpopulate missing submission/rank dates because the local metadata cache is missing.");
                return;
            }

            try
            {
                if (!localMetadataSource.IsAtLeastVersion(2))
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

            // find all ranked beatmap sets with missing date ranked or date submitted that have at least one difficulty ranked as well.
            // the reason for checking ranked status of the difficulties is that they can be locally modified or unknown too, and for those the lookup is likely to fail.
            // this is because metadata lookups are primarily based on file hash, so they will fail to match if the beatmap does not match the online version
            // (which is likely to be the case if the beatmap is locally modified or unknown).
            // that said, one difficulty in ranked state is enough for the backpopulation to work.
            HashSet<Guid> beatmapSetIds = realmAccess.Run(r => new HashSet<Guid>(
                r.All<BeatmapSetInfo>()
                 .Filter($@"{nameof(BeatmapSetInfo.StatusInt)} > 0 && ({nameof(BeatmapSetInfo.DateRanked)} == null || {nameof(BeatmapSetInfo.DateSubmitted)} == null) "
                         + $@"&& ANY {nameof(BeatmapSetInfo.Beatmaps)}.{nameof(BeatmapInfo.StatusInt)} > 0")
                 .AsEnumerable()
                 .Select(b => b.ID)));

            if (beatmapSetIds.Count == 0)
                return;

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets with missing submission/rank date.");

            var notification = showProgressNotification(beatmapSetIds.Count, "Populating missing submission and rank dates", "beatmap sets now have correct submission and rank dates.");

            const int chunk_size = 1000;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SqliteConnection sqliteConnection;
            int sqliteVersion;

            try
            {
                sqliteConnection = localMetadataSource.GetConnection();
                sqliteConnection.Open();

                sqliteVersion = localMetadataSource.GetCacheVersion(sqliteConnection);
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not open sqlite database: {ex}");
                return;
            }

            var actions = new List<Action<Realm>>();

            realmAccess.Run(r =>
            {
                foreach (Guid id in beatmapSetIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, beatmapSetIds.Count);

                    sleepIfRequired();

                    var beatmapSet = r.Find<BeatmapSetInfo>(id);

                    if (beatmapSet == null)
                    {
                        processedCount++;
                        continue;
                    }

                    try
                    {
                        var beatmap = beatmapSet.Beatmaps.First(b => b.Status >= BeatmapOnlineStatus.Ranked);

                        bool lookupSucceeded = localMetadataSource.TryLookup(sqliteConnection, sqliteVersion, beatmap, out var result);

                        if (!lookupSucceeded)
                        {
                            Logger.Log($"Could not find {beatmapSet.GetDisplayString()} in local cache while backpopulating missing submission/rank date");
                            ++failedCount;

                            continue;
                        }

                        Debug.Assert(result != null);

                        actions.Add(realm =>
                        {
                            var item = realm.Find<BeatmapSetInfo>(id)!;

                            item.DateRanked = result.DateRanked;
                            item.DateSubmitted = result.DateSubmitted;
                        });
                        ++processedCount;
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
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, beatmapSetIds.Count, failedCount);

            sqliteConnection.Close();

            Logger.Log($"Populating {processedCount} of {beatmapSetIds.Count} missing submission and rank dates completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private void backpopulateUserTags()
        {
            if (!localMetadataSource.Available || !localMetadataSource.IsAtLeastVersion(3))
            {
                Logger.Log(@"Local metadata cache has too low version to backpopulate user tags, attempting refetch...");
                localMetadataSource.FetchCache().WaitSafely();

                if (!localMetadataSource.Available || !localMetadataSource.IsAtLeastVersion(3))
                {
                    Logger.Log(@"Local metadata cache refetch failed. Aborting user tags backpopulation.");
                    return;
                }
            }

            var lastPopulation = config.Get<DateTime?>(OsuSetting.LastOnlineTagsPopulation);
            // dropping time data here completely is intentional, because storing the date to config is a lossy operation
            // (truncates some ticks off of the date when it's being converted to string and back).
            // therefore, if precision isn't explicitly constrained, the condition below would always fail just because the date stored to config
            // is less accurate than the cache file's fetch date which is stored with higher precision in the filesystem metadata.
            var metadataSourceFetchDate = localMetadataSource.GetCacheFetchDate()?.Date;

            if (metadataSourceFetchDate <= lastPopulation)
            {
                Logger.Log($@"Skipping user tag population because the local metadata source hasn't been updated since the last time user tags were checked ({lastPopulation.Value:d})");
                return;
            }

            Logger.Log(@"Querying for beatmaps that do not have user tags");

            // it is not an abnormal situation for a map not to have user tags.
            // while this is constrained to run every month or so (every time a new online.db cache is retrieved), there's some chance that this will still run much too often and be annoying to users.
            // if that turns out to be the case we may need a better way to debounce this (or just delete the backpopulation logic after some time has passed?)
            HashSet<Guid> beatmapIds = realmAccess.Run(r => new HashSet<Guid>(
                r.All<BeatmapInfo>()
                 .Filter($"{nameof(BeatmapInfo.Metadata)}.{nameof(BeatmapMetadata.UserTags)}.@count == 0 AND {nameof(BeatmapInfo.StatusInt)} IN {{ 1,2,4 }}")
                 .AsEnumerable()
                 .Select(b => b.ID)));

            if (beatmapIds.Count == 0)
                return;

            Logger.Log($@"Found {beatmapIds.Count} beatmaps with missing user tags.");

            var notification = showProgressNotification(beatmapIds.Count, @"Populating missing user tags", @"beatmaps have had their tags updated.");

            const int chunk_size = 2000;
            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var actions = new List<Action<Realm>>();

            SqliteConnection sqliteConnection;
            int sqliteVersion;

            try
            {
                sqliteConnection = localMetadataSource.GetConnection();
                sqliteConnection.Open();

                sqliteVersion = localMetadataSource.GetCacheVersion(sqliteConnection);
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not open sqlite database: {ex}");
                return;
            }

            realmAccess.Run(r =>
            {
                foreach (Guid id in beatmapIds)
                {
                    if (notification?.State == ProgressNotificationState.Cancelled)
                        break;

                    if (actions.Count >= chunk_size) performWrite(actions);

                    updateNotificationProgress(notification, processedCount, beatmapIds.Count);

                    sleepIfRequired();

                    var beatmap = r.Find<BeatmapInfo>(id);

                    if (beatmap == null)
                    {
                        processedCount++;
                        continue;
                    }

                    try
                    {
                        bool lookupSucceeded = localMetadataSource.TryLookup(sqliteConnection, sqliteVersion, beatmap, out var result);

                        if (!lookupSucceeded)
                        {
                            Logger.Log(@$"Could not find {beatmap.GetDisplayString()} in local cache while backpopulating missing user tags");
                            ++processedCount;

                            continue;
                        }

                        Debug.Assert(result != null);

                        var userTags = result.UserTags.ToHashSet();

                        if (userTags.SetEquals(beatmap.Metadata.UserTags))
                        {
                            ++processedCount;
                            continue;
                        }

                        actions.Add(realm =>
                        {
                            var item = realm.Find<BeatmapInfo>(id)!;

                            item.Metadata.UserTags.Clear();
                            item.Metadata.UserTags.AddRange(userTags);
                        });
                        ++processedCount;
                    }
                    catch (ObjectDisposedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Logger.Log(@$"Failed to update user tags for beatmap {id}: {e}");
                        ++failedCount;
                    }
                }
            });

            if (actions.Count > 0) performWrite(actions);

            completeNotification(notification, processedCount, beatmapIds.Count, failedCount);

            config.SetValue(OsuSetting.LastOnlineTagsPopulation, metadataSourceFetchDate);
            sqliteConnection.Close();

            Logger.Log($"Populating {processedCount} of {beatmapIds.Count} beatmap user tags completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private void performWrite(List<Action<Realm>> actions)
        {
            realmAccess.BulkWrite(actions);
            actions.Clear();
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
