// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly List<Action<Realm>> actions = [];

        protected virtual int TimeToSleepDuringGameplay => 30000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localMetadataSource = new LocalCachedBeatmapMetadataSource(storage);

            ProcessingTask = Task.Factory.StartNew(() =>
            {
                Logger.Log("Beginning background data store processing...");

                clearOutdatedStarRatings();
                populateMissingStarRatings();
                processOnlineBeatmapSetsWithNoUpdate();
                // Note that the previous method will also update these on a fresh run.
                processBeatmapsWithMissingObjectCounts();
                processScoresWithMissingStatistics();
                // ordering significant, `upgradeModMultipliers()` should run first as it will handle all scores
                // (rather than only lazer scores, if it was called after `convertLegacyTotalScoreToStandardised()`)
                upgradeModMultipliers();
                convertLegacyTotalScoreToStandardised();
                upgradeScoreRanks();
                backpopulateMissingSubmissionAndRankDates();
                backpopulateUserTags();
            }, TaskCreationOptions.LongRunning).ContinueWith(t =>
            {
                if (t.Exception?.InnerException is ObjectDisposedException)
                {
                    Logger.Log("Background data store processing aborted during shutdown.");
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
        private void populateMissingStarRatings(int chunkSize = 500)
        {
            Logger.Log("Querying for beatmaps with missing star ratings...");

            realmAccess.Run(r =>
            {
                Dictionary<string, Ruleset> rulesetCache = new Dictionary<string, Ruleset>();

                Ruleset getRuleset(RulesetInfo rulesetInfo)
                {
                    if (!rulesetCache.TryGetValue(rulesetInfo.ShortName, out var ruleset))
                        ruleset = rulesetCache[rulesetInfo.ShortName] = rulesetInfo.CreateInstance();

                    return ruleset;
                }

                var beatmaps = r.All<BeatmapInfo>().Where(b => b.StarRating < 0 && b.BeatmapSet != null);
                batchedProcessing(
                    items: beatmaps,
                    processItem: b =>
                    {
                        var beatmap = b.Detach();

                        var working = beatmapManager.GetWorkingBeatmap(beatmap);
                        var ruleset = getRuleset(working.BeatmapInfo.Ruleset);

                        Debug.Assert(ruleset != null);

                        var calculator = ruleset.CreateDifficultyCalculator(working);
                        double starRating = calculator.Calculate().StarRating;

                        return starRating;
                    },
                    saveItem: (realm, beatmap, starRating) =>
                    {
                        var item = realm.Find<BeatmapInfo>(beatmap.ID)!;
                        item.StarRating = starRating;

                        ((IWorkingBeatmapCache)beatmapManager).Invalidate(item);
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        FoundTemplate = "Found {total} beatmaps which require star rating reprocessing.",
                        RunningNotificationTemplate = "Reprocessing star rating for beatmaps",
                        CompletedNotificationTemplate = "beatmaps' star ratings have been updated",
                        ExceptionTemplate = "Calculating star rating failed",
                        FinishedTemplate = "Populating {processed} of {total} missing star ratings completed in {elapsed}ms"
                    }
                );
            });
        }

        private void processOnlineBeatmapSetsWithNoUpdate()
        {
            // BeatmapProcessor is responsible for both online and local processing.
            // In the case a user isn't logged in, it won't update LastOnlineUpdate and therefore re-queue,
            // causing overhead from the non-online processing to redundantly run every startup.
            //
            // We may eventually consider making the Process call more specific (or avoid this in any number
            // of other possible ways), but for now avoid queueing if the user isn't logged in at startup.
            if (!api.IsLoggedIn)
            {
                Logger.Log("Not logged in for beatmap sets to reprocess...");
                return;
            }

            HashSet<Guid> beatmapSetIds = new HashSet<Guid>();

            Logger.Log("Querying for beatmap sets to reprocess...");

            realmAccess.Run(r =>
            {
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.OnlineID > 0 && b.LastOnlineUpdate == null && b.BeatmapSet != null))
                    beatmapSetIds.Add(b.BeatmapSet!.ID);
            });

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets which require online updates.");

            if (beatmapSetIds.Count == 0) return;

            var notification = showProgressNotification(beatmapSetIds.Count, "Updating online data for beatmaps", "beatmaps' online data have been updated");

            int processedCount = 0;
            int failedCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (Guid id in beatmapSetIds)
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
                            Logger.Log($"Background processing failed on beatmap set {id}: {e}");
                            ++failedCount;
                        }
                    }
                });
            }

            completeNotification(notification, processedCount, beatmapSetIds.Count, failedCount);

            Logger.Log($"Processing {processedCount} of {beatmapSetIds.Count} online beatmap sets completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private void processBeatmapsWithMissingObjectCounts(int chunkSize = 500)
        {
            Logger.Log("Querying for beatmaps with missing hitobject counts to reprocess...");

            realmAccess.Run(r =>
            {
                var beatmaps = r.All<BeatmapInfo>().Where(b => b.TotalObjectCount < 0 || b.EndTimeObjectCount < 0);
                batchedProcessing<BeatmapInfo, BeatmapInfo>(
                    items: beatmaps,
                    processItem: b =>
                    {
                        var beatmap = b.Detach();
                        beatmapUpdater.ProcessObjectCounts(beatmap);

                        return beatmap;
                    },
                    saveItem: (realm, beatmap, result) =>
                    {
                        var item = realm.Find<BeatmapInfo>(beatmap.ID)!;

                        item.TotalObjectCount = result.TotalObjectCount;
                        item.EndTimeObjectCount = result.EndTimeObjectCount;

                        ((IWorkingBeatmapCache)beatmapManager).Invalidate(item);
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        FoundTemplate = "Found {total} beatmaps which require statistics population.",
                        RunningNotificationTemplate = "Populating missing statistics for beatmaps",
                        CompletedNotificationTemplate = "beatmaps have been populated with missing statistics",
                        ExceptionTemplate = "Calculating hitobject counts failed",
                        FinishedTemplate = "Processing {processed} of {total} missing beatmaps hitobject counts completed in {elapsed}ms"
                    }
                );
            });
        }

        private void processScoresWithMissingStatistics(int chunkSize = 1000)
        {
            Logger.Log("Querying for scores to reprocess...");

            realmAccess.Run(r =>
            {
                var scores = r.All<ScoreInfo>()
                              .Where(s => !s.BackgroundReprocessingFailed && s.BeatmapInfo != null)
                              .AsEnumerable()
                              // must be done after materialisation, as realm doesn't want to support
                              // nested property predicates
                              .Where(s => s.Statistics.Sum(kvp => kvp.Value) > 0
                                          && s.MaximumStatistics.Sum(kvp => kvp.Value) == 0);

                batchedProcessing<ScoreInfo, string>(
                    items: scores,
                    processItem: score =>
                    {
                        scoreManager.PopulateMaximumStatistics(score);
                        return JsonConvert.SerializeObject(score.MaximumStatistics);
                    },
                    saveItem: (realm, score, result) => realm.Find<ScoreInfo>(score.ID)!.MaximumStatisticsJson = result,
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        MarkAsFailedOnException = true,
                        FoundTemplate = "Found {total} scores which require statistics population.",
                        RunningNotificationTemplate = "Populating missing statistics for scores",
                        CompletedNotificationTemplate = "scores have been populated with missing statistics",
                        ExceptionTemplate = "Failed to populate maximum statistics",
                        FinishedTemplate = "Processing {processed} of {total} missing scores statistics completed in {elapsed}ms"
                    }
                );
            });
        }

        private void upgradeModMultipliers(int chunkSize = 500)
        {
            Logger.Log("Querying for scores that need mod multiplier upgrade...");

            realmAccess.Run(r =>
            {
                var scores = r.All<ScoreInfo>()
                              .Where(s => !s.BackgroundReprocessingFailed
                                          && s.BeatmapInfo != null
                                          && s.TotalScoreVersion < 30000017 // version number represents version with latest mod multiplier change
                                          && s.TotalScoreWithoutMods > 0)
                              .AsEnumerable()
                              // must be done after materialisation, as realm doesn't want to support
                              // nested property predicates
                              .Where(s => s.Ruleset.IsLegacyRuleset());

                batchedProcessing<ScoreInfo, ScoreInfo>(
                    items: scores,
                    processItem: s =>
                    {
                        var score = s.Detach();
                        if (score.BeatmapInfo == null)
                            return null;

                        StandardisedScoreMigrationTools.UpdateToLatestScoreMultipliers(score, score.BeatmapInfo.Difficulty);
                        return score;
                    },
                    saveItem: (realm, score, result) =>
                    {
                        var item = realm.Find<ScoreInfo>(score.ID)!;
                        item.TotalScore = result.TotalScore;
                        item.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        MarkAsFailedOnException = true,
                        FoundTemplate = "Found {total} scores which require mod multiplier upgrade.",
                        RunningNotificationTemplate = "Upgrading scores to new mod multipliers",
                        CompletedNotificationTemplate = "scores have been upgraded to the new mod multipliers",
                        ExceptionTemplate = "Failed to upgrade mod multipliers",
                        FinishedTemplate = "Upgrading {processed} of {total} scores to new mod multipliers completed in {elapsed}ms"
                    }
                );
            });
        }

        private void convertLegacyTotalScoreToStandardised(int chunkSize = 500)
        {
            Logger.Log("Querying for scores that need total score conversion...");

            realmAccess.Run(r =>
            {
                var scores = r.All<ScoreInfo>()
                              .Where(s => !s.BackgroundReprocessingFailed
                                          && s.BeatmapInfo != null
                                          && s.IsLegacyScore
                                          && s.TotalScoreVersion < LegacyScoreEncoder.LATEST_VERSION)
                              .AsEnumerable()
                              // must be done after materialisation, as realm doesn't want to support
                              // nested property predicates
                              .Where(s => s.Ruleset.IsLegacyRuleset());

                batchedProcessing<ScoreInfo, ScoreInfo>(
                    items: scores,
                    processItem: s =>
                    {
                        var score = s.Detach();
                        StandardisedScoreMigrationTools.UpdateFromLegacy(score, beatmapManager.GetWorkingBeatmap(score.BeatmapInfo));

                        return score;
                    },
                    saveItem: (realm, score, result) =>
                    {
                        var item = realm.Find<ScoreInfo>(score.ID)!;

                        item.Accuracy = result.Accuracy;
                        item.Rank = result.Rank;
                        item.TotalScore = result.TotalScore;
                        item.TotalScoreWithoutMods = result.TotalScoreWithoutMods;
                        item.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        MarkAsFailedOnException = true,
                        FoundTemplate = "Found {total} scores which require total score conversion.",
                        RunningNotificationTemplate = "Upgrading scores to new scoring algorithm",
                        CompletedNotificationTemplate = "scores have been upgraded to the new scoring algorithm",
                        ExceptionTemplate = "Failed to convert total score",
                        FinishedTemplate = "Converting total score {processed} of {total} scores completed in {elapsed}ms"
                    }
                );
            });
        }

        private void upgradeScoreRanks(int chunkSize = 3000)
        {
            Logger.Log("Querying for scores that need rank upgrades...");

            realmAccess.Run(r =>
            {
                var scores = r.All<ScoreInfo>()
                              .Where(s => s.TotalScoreVersion < 30000013 && !s.BackgroundReprocessingFailed) // last total score version with a significant change to ranks
                              .AsEnumerable()
                              // must be done after materialisation, as realm doesn't support
                              // filtering on nested property predicates or projection via `.Select()`
                              .Where(s => s.Ruleset.IsLegacyRuleset());

                batchedProcessing(
                    items: scores,
                    processItem: StandardisedScoreMigrationTools.ComputeRank,
                    saveItem: (realm, score, rank) =>
                    {
                        var item = realm.Find<ScoreInfo>(score.ID)!;

                        item.Rank = rank;
                        item.TotalScoreVersion = LegacyScoreEncoder.LATEST_VERSION;
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        MarkAsFailedOnException = true,
                        FoundTemplate = "Found {total} scores which require rank upgrades.",
                        RunningNotificationTemplate = "Adjusting ranks of scores",
                        CompletedNotificationTemplate = "scores now have more correct ranks.",
                        ExceptionTemplate = "Failed to update score rank",
                        FinishedTemplate = "Upgrading {processed} of {total} score ranks completed in {elapsed}ms"
                    }
                );
            });
        }

        private void backpopulateMissingSubmissionAndRankDates(int chunkSize = 1000)
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

            Logger.Log("Querying for beatmap sets that contain missing submission/rank dates...");

            localMetadataSource.CreateCachedConnection();

            // find all ranked beatmap sets with missing date ranked or date submitted that have at least one difficulty ranked as well.
            // the reason for checking ranked status of the difficulties is that they can be locally modified or unknown too, and for those the lookup is likely to fail.
            // this is because metadata lookups are primarily based on file hash, so they will fail to match if the beatmap does not match the online version
            // (which is likely to be the case if the beatmap is locally modified or unknown).
            // that said, one difficulty in ranked state is enough for the backpopulation to work.
            realmAccess.Run(r =>
            {
                var beatmapsSets = r.All<BeatmapSetInfo>()
                                    .Filter($@"{nameof(BeatmapSetInfo.StatusInt)} > 0 && ({nameof(BeatmapSetInfo.DateRanked)} == null || {nameof(BeatmapSetInfo.DateSubmitted)} == null) "
                                            + $@"&& ANY {nameof(BeatmapSetInfo.Beatmaps)}.{nameof(BeatmapInfo.StatusInt)} > 0")
                                    .AsEnumerable();

                batchedProcessing<BeatmapSetInfo, OnlineBeatmapMetadata>(
                    items: beatmapsSets,
                    processItem: beatmapSet =>
                    {
                        var beatmap = beatmapSet.Beatmaps.First(b => b.Status >= BeatmapOnlineStatus.Ranked);
                        bool lookupSucceeded = localMetadataSource.TryLookup(localMetadataSource.CachedConnection, localMetadataSource.CachedVersion, beatmap, out var result);

                        if (!lookupSucceeded)
                        {
                            Logger.Log($"Could not find {beatmapSet.GetDisplayString()} in local cache while backpopulating missing submission/rank date");
                            return null;
                        }

                        Debug.Assert(result != null);

                        return result;
                    },
                    saveItem: (realm, beatmapSet, result) =>
                    {
                        var item = realm.Find<BeatmapSetInfo>(beatmapSet.ID)!;

                        item.DateRanked = result.DateRanked;
                        item.DateSubmitted = result.DateSubmitted;
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        FoundTemplate = "Found {total} beatmap sets with missing submission/rank dates.",
                        RunningNotificationTemplate = "Populating missing submission and rank dates",
                        CompletedNotificationTemplate = "beatmap sets now have correct submission and rank dates.",
                        ExceptionTemplate = "Failed to update ranked/submitted dates for beatmap set {id}",
                        FinishedTemplate = "Populating {processed} of {total} missing submission and rank dates completed in {elapsed}ms"
                    }
                );
            });

            localMetadataSource.CachedConnection?.Close();
        }

        private void backpopulateUserTags(int chunkSize = 2000)
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
                Logger.Log(
                    $@"Skipping user tag population because the local metadata source hasn't been updated since the last time user tags were checked ({lastPopulation.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)})");
                return;
            }

            Logger.Log("Querying for beatmap that has outdated user tags...");

            localMetadataSource.CreateCachedConnection();

            // while this is constrained to run every month or so (every time a new online.db cache is retrieved), there's some chance that this will still run much too often and be annoying to users.
            // if that turns out to be the case we may need a better way to debounce this (or just delete the backpopulation logic after some time has passed?)
            realmAccess.Run(r =>
            {
                var beatmaps = r.All<BeatmapInfo>().Filter($"{nameof(BeatmapInfo.StatusInt)} IN {{ 1,2,4 }}");

                batchedProcessing<BeatmapInfo, HashSet<string>>(
                    items: beatmaps,
                    processItem: beatmap =>
                    {
                        bool lookupSucceeded = localMetadataSource.TryLookup(localMetadataSource.CachedConnection, localMetadataSource.CachedVersion, beatmap, out var result);

                        if (!lookupSucceeded)
                        {
                            Logger.Log(@$"Could not find {beatmap.GetDisplayString()} in local cache while backpopulating missing user tags");
                            return null;
                        }

                        Debug.Assert(result != null);

                        var userTags = result.UserTags.ToHashSet();

                        if (userTags.SetEquals(beatmap.Metadata.UserTags))
                        {
                            return null;
                        }

                        return userTags;
                    },
                    saveItem: (realm, beatmap, userTags) =>
                    {
                        var item = realm.Find<BeatmapInfo>(beatmap.ID)!;

                        item.Metadata.UserTags.Clear();
                        item.Metadata.UserTags.AddRange(userTags);
                    },
                    new BatchOptions
                    {
                        ChunkSize = chunkSize,
                        FoundTemplate = "Found beatmaps with outdated user tags.",
                        RunningNotificationTemplate = "Updating user tags",
                        CompletedNotificationTemplate = "beatmaps have had their tags updated. This runs once a month to allow searching user tags.",
                        ExceptionTemplate = "Failed to update user tags",
                        FinishedTemplate = "Populating {processed} of {total} beatmap user tags completed in {elapsed}ms"
                    }
                );
            });

            localMetadataSource.CachedConnection?.Close();
        }

        /// <summary>
        /// Helper method to process realm items in a batch of variable size to reduce writes to disk,
        /// and it speed up processes by huge amount,
        /// especially one's that run fast such as "upgradeScoreRanks" or "backpopulateMissingSubmissionAndRankDates".
        /// </summary>
        /// <typeparam name="T">a type of single item.</typeparam>
        /// <typeparam name="R">a return type for <paramref name="processItem"/></typeparam>
        /// <param name="items">collection of items to process.</param>
        /// <param name="processItem">a function that transforms each item <typeparamref name="T"/> into type <typeparamref name="R"/> or null.</param>
        /// <param name="saveItem">a function to save item in realm.Write transaction.</param>
        /// <param name="options">See <see cref="BatchOptions"/></param>
        private void batchedProcessing<T, R>(
            IEnumerable<T> items,
            Func<T, R?> processItem,
            Action<Realm, T, R> saveItem,
            BatchOptions options
        ) where T : RealmObjectBase
        {
            int totalCount = items.Count();
            int processedCount = 0;
            int failedCount = 0;

            Logger.Log(options.FoundTemplate.Replace("{total}", totalCount.ToString()));
            if (totalCount == 0) return;

            var notification = showProgressNotification(totalCount, options.RunningNotificationTemplate, options.CompletedNotificationTemplate);
            var stopwatch = Stopwatch.StartNew();

            foreach (var item in items)
            {
                if (notification?.State == ProgressNotificationState.Cancelled)
                    break;

                if (actions.Count >= options.ChunkSize) performWrite(actions);

                updateNotificationProgress(notification, processedCount + failedCount, totalCount);
                sleepIfRequired();

                // i don't know how to get ID in other way, please help
                var id = (item as ScoreInfo)?.ID;

                try
                {
                    var result = processItem(item);

                    if (result == null)
                    {
                        ++failedCount;
                        continue;
                    }

                    actions.Add(realm => saveItem(realm, item, result));
                    processedCount++;
                }
                catch (Exception e)
                {
                    Logger.Log($"{options.ExceptionTemplate.Replace("{id}", id.ToString())}: {e}");
                    if (options.MarkAsFailedOnException)
                        actions.Add(realm => realm.Find<ScoreInfo>(id)!.BackgroundReprocessingFailed = true);
                    ++failedCount;
                }
            }

            if (actions.Count > 0) performWrite(actions);
            completeNotification(notification, processedCount + failedCount, totalCount, failedCount);

            Logger.Log(options.FinishedTemplate.Replace("{processed}", processedCount.ToString())
                              .Replace("{total}", totalCount.ToString())
                              .Replace("{elapsed}", $"{stopwatch.ElapsedMilliseconds}ms"));
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

            if (totalCount == 0)
            {
                notification.CompleteSilently();
            }
            else if (processedCount == totalCount)
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

        private record BatchOptions
        {
            public int ChunkSize = 100;

            /// <summary>
            /// Sets <see cref="ScoreInfo.BackgroundReprocessingFailed"/> to true
            /// </summary>
            /// <remarks>Should be only used for <see cref="ScoreInfo"/>.</remarks>
            public bool MarkAsFailedOnException;

            /// <summary>
            /// Template for a log message of total found items.
            /// </summary>
            /// <remarks>Supports placeholders: {total}.</remarks>
            public string FoundTemplate = "";

            /// <summary>
            /// Template for notification message of running process.
            /// </summary>
            /// <remarks>No placeholders.</remarks>
            public string RunningNotificationTemplate = "";

            /// <summary>
            /// Template for notification message of completed process.
            /// </summary>
            /// <remarks>No placeholders.</remarks>
            public string CompletedNotificationTemplate = "";

            /// <summary>
            /// Template for a log message of an exception.
            /// </summary>
            /// <remarks>Supports placeholders: {id}.</remarks>
            public string ExceptionTemplate = "";

            /// <summary>
            /// Template for a log message of finished process.
            /// </summary>
            /// <remarks>Supports placeholders: {processed}, {total}, {elapsed}.</remarks>
            public string FinishedTemplate = "";
        }
    }
}
