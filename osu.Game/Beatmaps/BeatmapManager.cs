// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.IO;
using osu.Game.IPC;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    public partial class BeatmapManager
    {
        /// <summary>
        /// Fired when a new <see cref="BeatmapSetInfo"/> becomes available in the database.
        /// </summary>
        public event Action<BeatmapSetInfo> BeatmapSetAdded;

        /// <summary>
        /// Fired when a single difficulty has been hidden.
        /// </summary>
        public event Action<BeatmapInfo> BeatmapHidden;

        /// <summary>
        /// Fired when a <see cref="BeatmapSetInfo"/> is removed from the database.
        /// </summary>
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

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

        private readonly IDatabaseContextFactory contextFactory;

        private readonly FileStore files;

        private readonly RulesetStore rulesets;

        private readonly BeatmapStore beatmaps;

        private readonly APIAccess api;

        private readonly List<DownloadBeatmapSetRequest> currentDownloads = new List<DownloadBeatmapSetRequest>();

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private BeatmapIPCChannel ipc;

        /// <summary>
        /// Set an endpoint for notifications to be posted to.
        /// </summary>
        public Action<Notification> PostNotification { private get; set; }

        /// <summary>
        /// Set a storage with access to an osu-stable install for import purposes.
        /// </summary>
        public Func<Storage> GetStableStorage { private get; set; }

        public BeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, APIAccess api, IIpcHost importHost = null)
        {
            this.contextFactory = contextFactory;

            beatmaps = new BeatmapStore(contextFactory);

            beatmaps.BeatmapSetAdded += s => BeatmapSetAdded?.Invoke(s);
            beatmaps.BeatmapSetRemoved += s => BeatmapSetRemoved?.Invoke(s);
            beatmaps.BeatmapHidden += b => BeatmapHidden?.Invoke(b);
            beatmaps.BeatmapRestored += b => BeatmapRestored?.Invoke(b);

            files = new FileStore(contextFactory, storage);

            this.rulesets = rulesets;
            this.api = api;

            if (importHost != null)
                ipc = new BeatmapIPCChannel(importHost, this);

            beatmaps.Cleanup();
        }

        /// <summary>
        /// Import one or more <see cref="BeatmapSetInfo"/> from filesystem <paramref name="paths"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="paths">One or more beatmap locations on disk.</param>
        public List<BeatmapSetInfo> Import(params string[] paths)
        {
            var notification = new ProgressNotification
            {
                Text = "Beatmap import is initialising...",
                CompletionText = "Import successful!",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            List<BeatmapSetInfo> imported = new List<BeatmapSetInfo>();

            int i = 0;
            foreach (string path in paths)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return imported;

                try
                {
                    notification.Text = $"Importing ({i} of {paths.Length})\n{Path.GetFileName(path)}";
                    using (ArchiveReader reader = getReaderFrom(path))
                        imported.Add(Import(reader));

                    notification.Progress = (float)++i / paths.Length;

                    // We may or may not want to delete the file depending on where it is stored.
                    //  e.g. reconstructing/repairing database with beatmaps from default storage.
                    // Also, not always a single file, i.e. for LegacyFilesystemReader
                    // TODO: Add a check to prevent files from storage to be deleted.
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete original file after import ({Path.GetFileName(path)})");
                    }
                }
                catch (Exception e)
                {
                    e = e.InnerException ?? e;
                    Logger.Error(e, $@"Could not import beatmap set ({Path.GetFileName(path)})");
                }
            }

            notification.State = ProgressNotificationState.Completed;
            return imported;
        }

        /// <summary>
        /// Import a beatmap from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The beatmap to be imported.</param>
        public BeatmapSetInfo Import(ArchiveReader archive)
        {
            using (contextFactory.GetForWrite()) // used to share a context for full import. keep in mind this will block all writes.
            {
                // create a new set info (don't yet add to database)
                var beatmapSet = createBeatmapSetInfo(archive);

                // check if this beatmap has already been imported and exit early if so
                var existingHashMatch = beatmaps.BeatmapSets.FirstOrDefault(b => b.Hash == beatmapSet.Hash);
                if (existingHashMatch != null)
                {
                    Undelete(existingHashMatch);
                    return existingHashMatch;
                }

                // check if a set already exists with the same online id
                if (beatmapSet.OnlineBeatmapSetID != null)
                {
                    var existingOnlineId = beatmaps.BeatmapSets.FirstOrDefault(b => b.OnlineBeatmapSetID == beatmapSet.OnlineBeatmapSetID);
                    if (existingOnlineId != null)
                    {
                        Delete(existingOnlineId);
                        beatmaps.Cleanup(s => s.ID == existingOnlineId.ID);
                    }
                }

                beatmapSet.Files = createFileInfos(archive, files);
                beatmapSet.Beatmaps = createBeatmapDifficulties(archive);

                // remove metadata from difficulties where it matches the set
                foreach (BeatmapInfo b in beatmapSet.Beatmaps)
                    if (beatmapSet.Metadata.Equals(b.Metadata))
                        b.Metadata = null;

                // import to beatmap store
                Import(beatmapSet);
                return beatmapSet;
            }
        }

        /// <summary>
        /// Import a beatmap from a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to be imported.</param>
        public void Import(BeatmapSetInfo beatmapSet) => beatmaps.Add(beatmapSet);

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
                    using (var archive = new OszArchiveReader(stream))
                        Import(archive);

                    downloadNotification.State = ProgressNotificationState.Completed;
                    currentDownloads.Remove(request);
                }, TaskCreationOptions.LongRunning);
            };

            request.Failure += error =>
            {
                if (error is OperationCanceledException) return;

                downloadNotification.State = ProgressNotificationState.Completed;
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
        /// Update a BeatmapSetInfo with all changes. TODO: This only supports very basic updates currently.
        /// </summary>
        /// <param name="beatmapSet">The beatmap set to update.</param>
        public void Update(BeatmapSetInfo beatmap) => beatmaps.Update(beatmap);

        /// <summary>
        /// Delete a beatmap from the manager.
        /// Is a no-op for already deleted beatmaps.
        /// </summary>
        /// <param name="beatmapSet">The beatmap set to delete.</param>
        public void Delete(BeatmapSetInfo beatmapSet)
        {
            using (var usage = contextFactory.GetForWrite())
            {
                var context = usage.Context;

                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // re-fetch the beatmap set on the import context.
                beatmapSet = context.BeatmapSetInfo.Include(s => s.Files).ThenInclude(f => f.FileInfo).First(s => s.ID == beatmapSet.ID);

                if (beatmaps.Delete(beatmapSet))
                {
                    if (!beatmapSet.Protected)
                        files.Dereference(beatmapSet.Files.Select(f => f.FileInfo).ToArray());
                }

                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// Restore all beatmaps that were previously deleted.
        /// This will post notifications tracking progress.
        /// </summary>
        public void UndeleteAll()
        {
            var deleteMaps = QueryBeatmapSets(bs => bs.DeletePending).ToList();

            if (!deleteMaps.Any()) return;

            var notification = new ProgressNotification
            {
                CompletionText = "Restored all deleted beatmaps!",
                Progress = 0,
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            int i = 0;

            foreach (var bs in deleteMaps)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                notification.Text = $"Restoring ({i} of {deleteMaps.Count})";
                notification.Progress = (float)++i / deleteMaps.Count;
                Undelete(bs);
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Restore a beatmap that was previously deleted. Is a no-op if the beatmap is not in a deleted state, or has its protected flag set.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to restore</param>
        public void Undelete(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Protected)
                return;

            using (var usage = contextFactory.GetForWrite())
            {
                usage.Context.ChangeTracker.AutoDetectChangesEnabled = false;

                if (!beatmaps.Undelete(beatmapSet)) return;

                if (!beatmapSet.Protected)
                    files.Reference(beatmapSet.Files.Select(f => f.FileInfo).ToArray());

                usage.Context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

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

            WorkingBeatmap working = new BeatmapManagerWorkingBeatmap(files.Store, beatmapInfo);

            previous?.TransferTo(working);

            return working;
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.BeatmapSets.AsNoTracking().FirstOrDefault(query);

        /// <summary>
        /// Refresh an existing instance of a <see cref="BeatmapSetInfo"/> from the store.
        /// </summary>
        /// <param name="beatmapSet">A stale instance.</param>
        /// <returns>A fresh instance.</returns>
        public BeatmapSetInfo Refresh(BeatmapSetInfo beatmapSet) => QueryBeatmapSet(s => s.ID == beatmapSet.ID);

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets() => beatmaps.BeatmapSets.Where(s => !s.DeletePending).ToList();

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.BeatmapSets.AsNoTracking().Where(query);

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
        /// Delete all beatmaps.
        /// This will post notifications tracking progress.
        /// </summary>
        public void DeleteAll()
        {
            var maps = GetAllUsableBeatmapSets();

            if (maps.Count == 0) return;

            var notification = new ProgressNotification
            {
                Progress = 0,
                CompletionText = "Deleted all beatmaps!",
                State = ProgressNotificationState.Active,
            };

            PostNotification?.Invoke(notification);

            int i = 0;

            foreach (var b in maps)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                notification.Text = $"Deleting ({i} of {maps.Count})";
                notification.Progress = (float)++i / maps.Count;
                Delete(b);
            }

            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the beatmap content.</param>
        /// <returns>A reader giving access to the beatmap's content.</returns>
        private ArchiveReader getReaderFrom(string path)
        {
            if (ZipFile.IsZipFile(path))
                // ReSharper disable once InconsistentlySynchronizedField
                return new OszArchiveReader(files.Storage.GetStream(path));
            return new LegacyFilesystemReader(path);
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

        /// <summary>
        /// Create a <see cref="BeatmapSetInfo"/> from a provided archive.
        /// </summary>
        private BeatmapSetInfo createBeatmapSetInfo(ArchiveReader reader)
        {
            // let's make sure there are actually .osu files to import.
            string mapName = reader.Filenames.FirstOrDefault(f => f.EndsWith(".osu"));
            if (string.IsNullOrEmpty(mapName)) throw new InvalidOperationException("No beatmap files found in the map folder.");

            BeatmapMetadata metadata;
            using (var stream = new StreamReader(reader.GetStream(mapName)))
                metadata = Decoder.GetDecoder(stream).DecodeBeatmap(stream).Metadata;

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = metadata.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Hash = computeBeatmapSetHash(reader),
                Metadata = metadata
            };
        }

        /// <summary>
        /// Create all required <see cref="FileInfo"/>s for the provided archive, adding them to the global file store.
        /// </summary>
        private List<BeatmapSetFileInfo> createFileInfos(ArchiveReader reader, FileStore files)
        {
            List<BeatmapSetFileInfo> fileInfos = new List<BeatmapSetFileInfo>();

            // import files to manager
            foreach (string file in reader.Filenames)
                using (Stream s = reader.GetStream(file))
                    fileInfos.Add(new BeatmapSetFileInfo
                    {
                        Filename = file,
                        FileInfo = files.Add(s)
                    });

            return fileInfos;
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

                    var decoder = Decoder.GetDecoder(sr);
                    Beatmap beatmap = decoder.DecodeBeatmap(sr);

                    beatmap.BeatmapInfo.Path = name;
                    beatmap.BeatmapInfo.Hash = ms.ComputeSHA2Hash();
                    beatmap.BeatmapInfo.MD5Hash = ms.ComputeMD5Hash();

                    RulesetInfo ruleset = rulesets.GetRuleset(beatmap.BeatmapInfo.RulesetID);

                    // TODO: this should be done in a better place once we actually need to dynamically update it.
                    beatmap.BeatmapInfo.Ruleset = ruleset;
                    beatmap.BeatmapInfo.StarDifficulty = ruleset?.CreateInstance()?.CreateDifficultyCalculator(beatmap).Calculate() ?? 0;

                    beatmapInfos.Add(beatmap.BeatmapInfo);
                }
            }

            return beatmapInfos;
        }
    }
}
