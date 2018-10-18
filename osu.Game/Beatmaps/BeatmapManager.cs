// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Graphics;
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
        /// Fired when a beatmap load is requested (into the interactive game UI).
        /// </summary>
        public Action<BeatmapSetInfo> PresentBeatmap;

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public WorkingBeatmap DefaultBeatmap { private get; set; }

        public override string[] HandledExtensions => new[] { ".osz" };

        protected override string ImportFromStablePath => "Songs";

        private readonly RulesetStore rulesets;

        private readonly BeatmapStore beatmaps;

        private readonly APIAccess api;

        private readonly AudioManager audioManager;

        private readonly List<DownloadBeatmapSetRequest> currentDownloads = new List<DownloadBeatmapSetRequest>();

        public BeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, APIAccess api, AudioManager audioManager, IIpcHost importHost = null)
            : base(storage, contextFactory, new BeatmapStore(contextFactory), importHost)
        {
            beatmaps = (BeatmapStore)ModelStore;
            beatmaps.BeatmapHidden += b => BeatmapHidden?.Invoke(b);
            beatmaps.BeatmapRestored += b => BeatmapRestored?.Invoke(b);

            this.rulesets = rulesets;
            this.api = api;
            this.audioManager = audioManager;
        }

        protected override void Populate(BeatmapSetInfo beatmapSet, ArchiveReader archive)
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

            validateOnlineIds(beatmapSet.Beatmaps);

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
                fetchAndPopulateOnlineValues(b, beatmapSet.Beatmaps);

            // check if a set already exists with the same online id, delete if it does.
            if (beatmapSet.OnlineBeatmapSetID != null)
            {
                var existingOnlineId = beatmaps.ConsumableItems.FirstOrDefault(b => b.OnlineBeatmapSetID == beatmapSet.OnlineBeatmapSetID);
                if (existingOnlineId != null)
                {
                    Delete(existingOnlineId);
                    beatmaps.PurgeDeletable(s => s.ID == existingOnlineId.ID);
                    Logger.Log($"Found existing beatmap set with same OnlineBeatmapSetID ({beatmapSet.OnlineBeatmapSetID}). It has been purged.", LoggingTarget.Database);
                }
            }
        }

        private void validateOnlineIds(List<BeatmapInfo> beatmaps)
        {
            var beatmapIds = beatmaps.Where(b => b.OnlineBeatmapID.HasValue).Select(b => b.OnlineBeatmapID).ToList();

            // ensure all IDs are unique in this set and none match existing IDs in the local beatmap store.
            if (beatmapIds.GroupBy(b => b).Any(g => g.Count() > 1) || QueryBeatmaps(b => beatmapIds.Contains(b.OnlineBeatmapID)).Any())
                // remove all online IDs if any problems were found.
                beatmaps.ForEach(b => b.OnlineBeatmapID = null);
        }

        protected override BeatmapSetInfo CheckForExisting(BeatmapSetInfo model)
        {
            // check if this beatmap has already been imported and exit early if so
            var existingHashMatch = beatmaps.ConsumableItems.FirstOrDefault(b => b.Hash == model.Hash);
            if (existingHashMatch != null)
            {
                Undelete(existingHashMatch);
                return existingHashMatch;
            }

            return null;
        }

        /// <summary>
        /// Downloads a beatmap.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to be downloaded.</param>
        /// <param name="noVideo">Whether the beatmap should be downloaded without video. Defaults to false.</param>
        public void Download(BeatmapSetInfo beatmapSetInfo, bool noVideo = false)
        {
            var existing = GetExistingDownload(beatmapSetInfo);

            if (existing != null || api == null) return;

            if (!api.LocalUser.Value.IsSupporter)
            {
                PostNotification?.Invoke(new SimpleNotification
                {
                    Icon = FontAwesome.fa_superpowers,
                    Text = "You gotta be an osu!supporter to download for now 'yo"
                });
                return;
            }

            var downloadNotification = new DownloadNotification
            {
                CompletionText = $"Imported {beatmapSetInfo.Metadata.Artist} - {beatmapSetInfo.Metadata.Title}!",
                Text = $"Downloading {beatmapSetInfo.Metadata.Artist} - {beatmapSetInfo.Metadata.Title}",
            };

            var request = new DownloadBeatmapSetRequest(beatmapSetInfo, noVideo);

            request.DownloadProgressed += progress =>
            {
                downloadNotification.State = ProgressNotificationState.Active;
                downloadNotification.Progress = progress;
            };

            request.Success += data =>
            {
                downloadNotification.Text = $"Importing {beatmapSetInfo.Metadata.Artist} - {beatmapSetInfo.Metadata.Title}";

                Task.Factory.StartNew(() =>
                {
                    BeatmapSetInfo importedBeatmap;

                    // This gets scheduled back to the update thread, but we want the import to run in the background.
                    using (var stream = new MemoryStream(data))
                    using (var archive = new ZipArchiveReader(stream, beatmapSetInfo.ToString()))
                        importedBeatmap = Import(archive);

                    downloadNotification.CompletionClickAction = () =>
                    {
                        PresentCompletedImport(importedBeatmap.Yield());
                        return true;
                    };
                    downloadNotification.State = ProgressNotificationState.Completed;

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
            Task.Factory.StartNew(() => request.Perform(api), TaskCreationOptions.LongRunning);
            BeatmapDownloadBegan?.Invoke(request);
        }

        protected override void PresentCompletedImport(IEnumerable<BeatmapSetInfo> imported)
        {
            base.PresentCompletedImport(imported);
            PresentBeatmap?.Invoke(imported.LastOrDefault());
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
        /// <param name="previous">The currently loaded <see cref="WorkingBeatmap"/>. Allows for optimisation where elements are shared with the new beatmap.</param>
        /// <returns>A <see cref="WorkingBeatmap"/> instance correlating to the provided <see cref="BeatmapInfo"/>.</returns>
        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null)
        {
            if (beatmapInfo?.BeatmapSet == null || beatmapInfo == DefaultBeatmap?.BeatmapInfo)
                return DefaultBeatmap;

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapInfo.BeatmapSet.Metadata;

            WorkingBeatmap working = new BeatmapManagerWorkingBeatmap(Files.Store, beatmapInfo, audioManager);

            previous?.TransferTo(working);

            return working;
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.ConsumableItems.AsNoTracking().FirstOrDefault(query);

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

        /// <summary>
        /// Create a SHA-2 hash from the provided archive based on contained beatmap (.osu) file content.
        /// </summary>
        private string computeBeatmapSetHash(ArchiveReader reader)
        {
            // for now, concatenate all .osu files in the set to create a unique hash.
            MemoryStream hashable = new MemoryStream();
            foreach (string file in reader.Filenames.Where(f => f.EndsWith(".osu")))
                using (Stream s = reader.GetStream(file))
                    s.CopyTo(hashable);

            return hashable.ComputeSHA2Hash();
        }

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
                Hash = computeBeatmapSetHash(reader),
                Metadata = beatmap.Metadata
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
        /// Query the API to populate missing values like OnlineBeatmapID / OnlineBeatmapSetID or (Rank-)Status.
        /// </summary>
        /// <param name="beatmap">The beatmap to populate.</param>
        /// <param name="otherBeatmaps">The other beatmaps contained within this set.</param>
        /// <param name="force">Whether to re-query if the provided beatmap already has populated values.</param>
        /// <returns>True if population was successful.</returns>
        private bool fetchAndPopulateOnlineValues(BeatmapInfo beatmap, IEnumerable<BeatmapInfo> otherBeatmaps, bool force = false)
        {
            if (api?.State != APIState.Online)
                return false;

            if (!force && beatmap.OnlineBeatmapID != null && beatmap.BeatmapSet.OnlineBeatmapSetID != null
                && beatmap.Status != BeatmapSetOnlineStatus.None && beatmap.BeatmapSet.Status != BeatmapSetOnlineStatus.None)
                return true;

            Logger.Log("Attempting online lookup for the missing values...", LoggingTarget.Database);

            try
            {
                var req = new GetBeatmapRequest(beatmap);

                req.Perform(api);

                var res = req.Result;

                Logger.Log($"Successfully mapped to {res.OnlineBeatmapSetID} / {res.OnlineBeatmapID}.", LoggingTarget.Database);

                beatmap.Status = res.Status;
                beatmap.BeatmapSet.Status = res.BeatmapSet.Status;

                if (otherBeatmaps.Any(b => b.OnlineBeatmapID == res.OnlineBeatmapID))
                {
                    Logger.Log("Another beatmap in the same set already mapped to this ID. We'll skip adding it this time.", LoggingTarget.Database);
                    return false;
                }

                beatmap.BeatmapSet.OnlineBeatmapSetID = res.OnlineBeatmapSetID;
                beatmap.OnlineBeatmapID = res.OnlineBeatmapID;

                return true;
            }
            catch (Exception e)
            {
                Logger.Log($"Failed ({e})", LoggingTarget.Database);
                return false;
            }
        }

        /// <summary>
        /// A dummy WorkingBeatmap for the purpose of retrieving a beatmap for star difficulty calculation.
        /// </summary>
        private class DummyConversionBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public DummyConversionBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo)
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
    }
}
