// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    public partial class BeatmapManager : ArchiveModelManager<BeatmapSetInfo, BeatmapSetFileInfo>
    {
        /// <summary>
        /// Fired when a single difficulty has been hidden.
        /// </summary>
        public event Action<BeatmapInfo> BeatmapHidden;

        /// <summary>
        /// Fired when a single difficulty has been restored.
        /// </summary>
        public event Action<BeatmapInfo> BeatmapRestored;

        /// <summary>
        /// Fired when a beatmap download begins.
        /// </summary>
        public event Action<DownloadBeatmapSetRequest> BeatmapDownloadBegan;

        /// <summary>
        /// Fired when a beatmap download is interrupted, due to user cancellation or other failures.
        /// </summary>
        public event Action<DownloadBeatmapSetRequest> BeatmapDownloadFailed;

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public readonly WorkingBeatmap DefaultBeatmap;

        public override string[] HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        protected override string ImportFromStablePath => "Songs";

        private readonly RulesetStore rulesets;

        private readonly BeatmapStore beatmaps;

        private readonly IAPIProvider api;

        private readonly AudioManager audioManager;

        private readonly GameHost host;

        private readonly List<DownloadBeatmapSetRequest> currentDownloads = new List<DownloadBeatmapSetRequest>();

        private readonly BeatmapUpdateQueue updateQueue;

        public BeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, AudioManager audioManager, GameHost host = null,
                              WorkingBeatmap defaultBeatmap = null)
            : base(storage, contextFactory, new BeatmapStore(contextFactory), host)
        {
            this.rulesets = rulesets;
            this.api = api;
            this.audioManager = audioManager;
            this.host = host;

            DefaultBeatmap = defaultBeatmap;

            beatmaps = (BeatmapStore)ModelStore;
            beatmaps.BeatmapHidden += b => BeatmapHidden?.Invoke(b);
            beatmaps.BeatmapRestored += b => BeatmapRestored?.Invoke(b);

            updateQueue = new BeatmapUpdateQueue(api);
        }

        protected override Task Populate(BeatmapSetInfo beatmapSet, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            if (archive != null)
                beatmapSet.Beatmaps = createBeatmapDifficulties(archive);

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                // remove metadata from difficulties where it matches the set
                if (beatmapSet.Metadata.Equals(b.Metadata))
                    b.Metadata = null;

                b.BeatmapSet = beatmapSet;
            }

            validateOnlineIds(beatmapSet);

            return updateQueue.UpdateAsync(beatmapSet, cancellationToken);
        }

        protected override void PreImport(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Beatmaps.Any(b => b.BaseDifficulty == null))
                throw new InvalidOperationException($"Cannot import {nameof(BeatmapInfo)} with null {nameof(BeatmapInfo.BaseDifficulty)}.");

            // check if a set already exists with the same online id, delete if it does.
            if (beatmapSet.OnlineBeatmapSetID != null)
            {
                var existingOnlineId = beatmaps.ConsumableItems.FirstOrDefault(b => b.OnlineBeatmapSetID == beatmapSet.OnlineBeatmapSetID);

                if (existingOnlineId != null)
                {
                    Delete(existingOnlineId);
                    beatmaps.PurgeDeletable(s => s.ID == existingOnlineId.ID);
                    LogForModel(beatmapSet, $"Found existing beatmap set with same OnlineBeatmapSetID ({beatmapSet.OnlineBeatmapSetID}). It has been purged.");
                }
            }
        }

        private void validateOnlineIds(BeatmapSetInfo beatmapSet)
        {
            var beatmapIds = beatmapSet.Beatmaps.Where(b => b.OnlineBeatmapID.HasValue).Select(b => b.OnlineBeatmapID).ToList();

            // ensure all IDs are unique
            if (beatmapIds.GroupBy(b => b).Any(g => g.Count() > 1))
            {
                resetIds();
                return;
            }

            // find any existing beatmaps in the database that have matching online ids
            var existingBeatmaps = QueryBeatmaps(b => beatmapIds.Contains(b.OnlineBeatmapID)).ToList();

            if (existingBeatmaps.Count > 0)
            {
                // reset the import ids (to force a re-fetch) *unless* they match the candidate CheckForExisting set.
                // we can ignore the case where the new ids are contained by the CheckForExisting set as it will either be used (import skipped) or deleted.
                var existing = CheckForExisting(beatmapSet);
                if (existing == null || existingBeatmaps.Any(b => !existing.Beatmaps.Contains(b)))
                    resetIds();
            }

            void resetIds() => beatmapSet.Beatmaps.ForEach(b => b.OnlineBeatmapID = null);
        }

        /// <summary>
        /// Downloads a beatmap.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to be downloaded.</param>
        /// <param name="noVideo">Whether the beatmap should be downloaded without video. Defaults to false.</param>
        /// <returns>Downloading can happen</returns>
        public bool Download(BeatmapSetInfo beatmapSetInfo, bool noVideo = false)
        {
            var existing = GetExistingDownload(beatmapSetInfo);

            if (existing != null || api == null) return false;

            var downloadNotification = new DownloadNotification
            {
                Text = $"Downloading {beatmapSetInfo}",
            };

            var request = new DownloadBeatmapSetRequest(beatmapSetInfo, noVideo);

            request.DownloadProgressed += progress =>
            {
                downloadNotification.State = ProgressNotificationState.Active;
                downloadNotification.Progress = progress;
            };

            request.Success += filename =>
            {
                Task.Factory.StartNew(async () =>
                {
                    // This gets scheduled back to the update thread, but we want the import to run in the background.
                    await Import(downloadNotification, filename);
                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += error =>
            {
                BeatmapDownloadFailed?.Invoke(request);

                if (error is OperationCanceledException) return;

                downloadNotification.State = ProgressNotificationState.Cancelled;
                Logger.Error(error, "Beatmap download failed!");
                currentDownloads.Remove(request);
            };

            downloadNotification.CancelRequested += () =>
            {
                request.Cancel();
                currentDownloads.Remove(request);
                downloadNotification.State = ProgressNotificationState.Cancelled;
                return true;
            };

            currentDownloads.Add(request);
            PostNotification?.Invoke(downloadNotification);

            // don't run in the main api queue as this is a long-running task.
            Task.Factory.StartNew(() =>
            {
                try
                {
                    request.Perform(api);
                }
                catch
                {
                    // no need to handle here as exceptions will filter down to request.Failure above.
                }
            }, TaskCreationOptions.LongRunning);
            BeatmapDownloadBegan?.Invoke(request);
            return true;
        }

        /// <summary>
        /// Get an existing download request if it exists.
        /// </summary>
        /// <param name="beatmap">The <see cref="BeatmapSetInfo"/> whose download request is wanted.</param>
        /// <returns>The <see cref="DownloadBeatmapSetRequest"/> object if it exists, or null.</returns>
        public DownloadBeatmapSetRequest GetExistingDownload(BeatmapSetInfo beatmap) => currentDownloads.Find(d => d.BeatmapSet.OnlineBeatmapSetID == beatmap.OnlineBeatmapSetID);

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmap">The beatmap difficulty to hide.</param>
        public void Hide(BeatmapInfo beatmap) => beatmaps.Hide(beatmap);

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmap">The beatmap difficulty to restore.</param>
        public void Restore(BeatmapInfo beatmap) => beatmaps.Restore(beatmap);

        /// <summary>
        /// Retrieve a <see cref="WorkingBeatmap"/> instance for the provided <see cref="BeatmapInfo"/>
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to lookup.</param>
        /// <param name="previous">The currently loaded <see cref="WorkingBeatmap"/>. Allows for optimisation where elements are shared with the new beatmap. May be returned if beatmapInfo requested matches</param>
        /// <returns>A <see cref="WorkingBeatmap"/> instance correlating to the provided <see cref="BeatmapInfo"/>.</returns>
        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null)
        {
            if (beatmapInfo?.ID > 0 && previous != null && previous.BeatmapInfo?.ID == beatmapInfo.ID)
                return previous;

            if (beatmapInfo?.BeatmapSet == null || beatmapInfo == DefaultBeatmap?.BeatmapInfo)
                return DefaultBeatmap;

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapInfo.BeatmapSet.Metadata;

            WorkingBeatmap working = new BeatmapManagerWorkingBeatmap(Files.Store, new LargeTextureStore(host?.CreateTextureLoaderStore(Files.Store)), beatmapInfo, audioManager);

            previous?.TransferTo(working);

            return working;
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override bool CanUndelete(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanUndelete(existing, import))
                return false;

            var existingIds = existing.Beatmaps.Select(b => b.OnlineBeatmapID).OrderBy(i => i);
            var importIds = import.Beatmaps.Select(b => b.OnlineBeatmapID).OrderBy(i => i);

            // force re-import if we are not in a sane state.
            return existing.OnlineBeatmapSetID == import.OnlineBeatmapSetID && existingIds.SequenceEqual(importIds);
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets() => GetAllUsableBeatmapSetsEnumerable().ToList();

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public IQueryable<BeatmapSetInfo> GetAllUsableBeatmapSetsEnumerable() => beatmaps.ConsumableItems.Where(s => !s.DeletePending && !s.Protected);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.ConsumableItems.AsNoTracking().Where(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().FirstOrDefault(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().Where(query);

        protected override string HumanisedModelName => "beatmap";

        protected override BeatmapSetInfo CreateModel(ArchiveReader reader)
        {
            // let's make sure there are actually .osu files to import.
            string mapName = reader.Filenames.FirstOrDefault(f => f.EndsWith(".osu"));

            if (string.IsNullOrEmpty(mapName))
            {
                Logger.Log($"No beatmap files found in the beatmap archive ({reader.Name}).", LoggingTarget.Database);
                return null;
            }

            Beatmap beatmap;
            using (var stream = new StreamReader(reader.GetStream(mapName)))
                beatmap = Decoder.GetDecoder<Beatmap>(stream).Decode(stream);

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = beatmap.BeatmapInfo.BeatmapSet?.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Metadata = beatmap.Metadata,
                DateAdded = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Create all required <see cref="BeatmapInfo"/>s for the provided archive.
        /// </summary>
        private List<BeatmapInfo> createBeatmapDifficulties(ArchiveReader reader)
        {
            var beatmapInfos = new List<BeatmapInfo>();

            foreach (var name in reader.Filenames.Where(f => f.EndsWith(".osu")))
            {
                using (var raw = reader.GetStream(name))
                using (var ms = new MemoryStream()) //we need a memory stream so we can seek and shit
                using (var sr = new StreamReader(ms))
                {
                    raw.CopyTo(ms);
                    ms.Position = 0;

                    var decoder = Decoder.GetDecoder<Beatmap>(sr);
                    IBeatmap beatmap = decoder.Decode(sr);

                    beatmap.BeatmapInfo.Path = name;
                    beatmap.BeatmapInfo.Hash = ms.ComputeSHA2Hash();
                    beatmap.BeatmapInfo.MD5Hash = ms.ComputeMD5Hash();

                    var ruleset = rulesets.GetRuleset(beatmap.BeatmapInfo.RulesetID);
                    beatmap.BeatmapInfo.Ruleset = ruleset;
                    // TODO: this should be done in a better place once we actually need to dynamically update it.
                    beatmap.BeatmapInfo.StarDifficulty = ruleset?.CreateInstance().CreateDifficultyCalculator(new DummyConversionBeatmap(beatmap)).Calculate().StarRating ?? 0;

                    beatmapInfos.Add(beatmap.BeatmapInfo);
                }
            }

            return beatmapInfos;
        }

        /// <summary>
        /// A dummy WorkingBeatmap for the purpose of retrieving a beatmap for star difficulty calculation.
        /// </summary>
        private class DummyConversionBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public DummyConversionBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;
            protected override Texture GetBackground() => null;
            protected override Track GetTrack() => null;
        }

        private class DownloadNotification : ProgressNotification
        {
            public override bool IsImportant => false;

            protected override Notification CreateCompletionNotification() => new SilencedProgressCompletionNotification
            {
                Activated = CompletionClickAction,
                Text = CompletionText
            };

            private class SilencedProgressCompletionNotification : ProgressCompletionNotification
            {
                public override bool IsImportant => false;
            }
        }

        private class BeatmapUpdateQueue
        {
            private readonly IAPIProvider api;

            private const int update_queue_request_concurrency = 4;

            private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(update_queue_request_concurrency, nameof(BeatmapUpdateQueue));

            public BeatmapUpdateQueue(IAPIProvider api)
            {
                this.api = api;
            }

            public Task UpdateAsync(BeatmapSetInfo beatmapSet, CancellationToken cancellationToken)
            {
                if (api?.State != APIState.Online)
                    return Task.CompletedTask;

                LogForModel(beatmapSet, "Performing online lookups...");
                return Task.WhenAll(beatmapSet.Beatmaps.Select(b => UpdateAsync(beatmapSet, b, cancellationToken)).ToArray());
            }

            // todo: expose this when we need to do individual difficulty lookups.
            protected Task UpdateAsync(BeatmapSetInfo beatmapSet, BeatmapInfo beatmap, CancellationToken cancellationToken)
                => Task.Factory.StartNew(() => update(beatmapSet, beatmap), cancellationToken, TaskCreationOptions.HideScheduler, updateScheduler);

            private void update(BeatmapSetInfo set, BeatmapInfo beatmap)
            {
                if (api?.State != APIState.Online)
                    return;

                var req = new GetBeatmapRequest(beatmap);

                req.Success += res =>
                {
                    LogForModel(set, $"Online retrieval mapped {beatmap} to {res.OnlineBeatmapSetID} / {res.OnlineBeatmapID}.");

                    beatmap.Status = res.Status;
                    beatmap.BeatmapSet.Status = res.BeatmapSet.Status;
                    beatmap.BeatmapSet.OnlineBeatmapSetID = res.OnlineBeatmapSetID;
                    beatmap.OnlineBeatmapID = res.OnlineBeatmapID;
                };

                req.Failure += e => { LogForModel(set, $"Online retrieval failed for {beatmap}", e); };

                // intentionally blocking to limit web request concurrency
                req.Perform(api);
            }
        }
    }
}
