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
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public WorkingBeatmap DefaultBeatmap { private get; set; }

        public override string[] HandledExtensions => new[] { ".osz" };

        private readonly RulesetStore rulesets;

        private readonly BeatmapStore beatmaps;

        private readonly APIAccess api;

        private readonly AudioManager audioManager;

        private readonly List<DownloadBeatmapSetRequest> currentDownloads = new List<DownloadBeatmapSetRequest>();

        /// <summary>
        /// Set a storage with access to an osu-stable install for import purposes.
        /// </summary>
        public Func<Storage> GetStableStorage { private get; set; }

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

        protected override void Populate(BeatmapSetInfo model, ArchiveReader archive)
        {
            model.Beatmaps = createBeatmapDifficulties(model, archive);

            // remove metadata from difficulties where it matches the set
            foreach (BeatmapInfo b in model.Beatmaps)
                if (model.Metadata.Equals(b.Metadata))
                    b.Metadata = null;
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

            // check if a set already exists with the same online id
            if (model.OnlineBeatmapSetID != null)
            {
                var existingOnlineId = beatmaps.ConsumableItems.FirstOrDefault(b => b.OnlineBeatmapSetID == model.OnlineBeatmapSetID);
                if (existingOnlineId != null)
                {
                    Delete(existingOnlineId);
                    beatmaps.PurgeDeletable(s => s.ID == existingOnlineId.ID);
                    Logger.Log($"Found existing beatmap set with same OnlineBeatmapSetID ({model.OnlineBeatmapSetID}). It has been purged.", LoggingTarget.Database);
                }
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
                    Text = "You gotta be a supporter to download for now 'yo"
                });
                return;
            }

            var downloadNotification = new ProgressNotification
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
                    // This gets scheduled back to the update thread, but we want the import to run in the background.
                    using (var stream = new MemoryStream(data))
                    using (var archive = new ZipArchiveReader(stream, beatmapSetInfo.ToString()))
                        Import(archive);

                    downloadNotification.State = ProgressNotificationState.Completed;
                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += error =>
            {
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
        public IEnumerable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().Where(query);

        /// <summary>
        /// Denotes whether an osu-stable installation is present to perform automated imports from.
        /// </summary>
        public bool StableInstallationAvailable => GetStableStorage?.Invoke() != null;

        /// <summary>
        /// This is a temporary method and will likely be replaced by a full-fledged (and more correctly placed) migration process in the future.
        /// </summary>
        public async Task ImportFromStable()
        {
            var stable = GetStableStorage?.Invoke();

            if (stable == null)
            {
                Logger.Log("No osu!stable installation available!", LoggingTarget.Information, LogLevel.Error);
                return;
            }

            await Task.Factory.StartNew(() => Import(stable.GetDirectories("Songs")), TaskCreationOptions.LongRunning);
        }

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
            if (string.IsNullOrEmpty(mapName)) throw new InvalidOperationException("No beatmap files found in this beatmap archive.");

            BeatmapMetadata metadata;
            using (var stream = new StreamReader(reader.GetStream(mapName)))
                metadata = Decoder.GetDecoder<Beatmap>(stream).Decode(stream).Metadata;

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = metadata.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Hash = computeBeatmapSetHash(reader),
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create all required <see cref="BeatmapInfo"/>s for the provided archive.
        /// </summary>
        private List<BeatmapInfo> createBeatmapDifficulties(BeatmapSetInfo model, ArchiveReader reader)
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

                    // ensure we have the same online set ID as the set itself.
                    beatmap.BeatmapInfo.OnlineBeatmapSetID = model.OnlineBeatmapSetID;
                    beatmap.BeatmapInfo.Metadata.OnlineBeatmapSetID = model.OnlineBeatmapSetID;

                    // check that no existing beatmap exists that is imported with the same online beatmap ID. if so, give it precedence.
                    if (beatmap.BeatmapInfo.OnlineBeatmapID.HasValue && QueryBeatmap(b => b.OnlineBeatmapID.Value == beatmap.BeatmapInfo.OnlineBeatmapID.Value) != null)
                        beatmap.BeatmapInfo.OnlineBeatmapID = null;

                    RulesetInfo ruleset = rulesets.GetRuleset(beatmap.BeatmapInfo.RulesetID);

                    beatmap.BeatmapInfo.Ruleset = ruleset;

                    if (ruleset != null)
                    {
                        // TODO: this should be done in a better place once we actually need to dynamically update it.
                        var converted = new DummyConversionBeatmap(beatmap).GetPlayableBeatmap(ruleset);
                        beatmap.BeatmapInfo.StarDifficulty = ruleset.CreateInstance().CreateDifficultyCalculator(converted).Calculate();
                    }
                    else
                        beatmap.BeatmapInfo.StarDifficulty = 0;

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
                : base(beatmap.BeatmapInfo)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;
            protected override Texture GetBackground() => null;
            protected override Track GetTrack() => null;
        }
    }
}
