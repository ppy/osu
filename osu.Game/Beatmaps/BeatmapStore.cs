// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Ionic.Zip;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.IO;
using osu.Game.IPC;
using osu.Game.Rulesets;
using SQLite.Net;
using FileInfo = osu.Game.IO.FileInfo;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    public class BeatmapStore
    {
        /// <summary>
        /// Fired when a new <see cref="BeatmapSetInfo"/> becomes available in the database.
        /// </summary>
        public event Action<BeatmapSetInfo> BeatmapSetAdded;

        /// <summary>
        /// Fired when a <see cref="BeatmapSetInfo"/> is removed from the database.
        /// </summary>
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public WorkingBeatmap DefaultBeatmap { private get; set; }

        private readonly Storage storage;

        private readonly FileDatabase files;

        private readonly RulesetDatabase rulesets;

        private readonly BeatmapDatabase beatmaps;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private BeatmapIPCChannel ipc;

        public BeatmapStore(Storage storage, FileDatabase files, SQLiteConnection connection, RulesetDatabase rulesets, IIpcHost importHost = null)
        {
            beatmaps = new BeatmapDatabase(connection);
            beatmaps.BeatmapSetAdded += s => BeatmapSetAdded?.Invoke(s);
            beatmaps.BeatmapSetRemoved += s => BeatmapSetRemoved?.Invoke(s);

            this.storage = storage;
            this.files = files;
            this.rulesets = rulesets;

            if (importHost != null)
                ipc = new BeatmapIPCChannel(importHost, this);
        }

        /// <summary>
        /// Import multiple <see cref="BeatmapSetInfo"/> from filesystem <paramref name="paths"/>.
        /// </summary>
        /// <param name="paths">Multiple locations on disk.</param>
        public void Import(params string[] paths)
        {
            foreach (string path in paths)
            {
                try
                {
                    using (ArchiveReader reader = getReaderFrom(path))
                        Import(reader);

                    // We may or may not want to delete the file depending on where it is stored.
                    //  e.g. reconstructing/repairing database with beatmaps from default storage.
                    // Also, not always a single file, i.e. for LegacyFilesystemReader
                    // TODO: Add a check to prevent files from storage to be deleted.
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete file at {path}");
                    }
                }
                catch (Exception e)
                {
                    e = e.InnerException ?? e;
                    Logger.Error(e, @"Could not import beatmap set");
                }
            }
        }

        /// <summary>
        /// Import a beatmap from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archiveReader">The beatmap to be imported.</param>
        public BeatmapSetInfo Import(ArchiveReader archiveReader)
        {
            BeatmapSetInfo set = importToStorage(archiveReader);
            Import(set);
            return set;
        }

        /// <summary>
        /// Import a beatmap from a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSetInfo">The beatmap to be imported.</param>
        public void Import(BeatmapSetInfo beatmapSetInfo)
        {
            // If we have an ID then we already exist in the database.
            if (beatmapSetInfo.ID != 0) return;

            beatmaps.Add(beatmapSetInfo);
        }

        /// <summary>
        /// Delete a beatmap from the store.
        /// Is a no-op for already deleted beatmaps.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to delete.</param>
        public void Delete(BeatmapSetInfo beatmapSet)
        {
            if (!beatmaps.Delete(beatmapSet)) return;

            if (!beatmapSet.Protected)
                files.Dereference(beatmapSet.Files);
        }

        /// <summary>
        /// Returns a <see cref="BeatmapSetInfo"/> to a usable state if it has previously been deleted but not yet purged.
        /// Is a no-op for already usable beatmaps.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to restore.</param>
        public void Undelete(BeatmapSetInfo beatmapSet)
        {
            if (!beatmaps.Undelete(beatmapSet)) return;

            files.Reference(beatmapSet.Files);
        }

        /// <summary>
        /// Retrieve a <see cref="WorkingBeatmap"/> instance for the provided <see cref="BeatmapInfo"/>
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to lookup.</param>
        /// <param name="previous">The currently loaded <see cref="WorkingBeatmap"/>. Allows for optimisation where elements are shared with the new beatmap.</param>
        /// <returns>A <see cref="WorkingBeatmap"/> instance correlating to the provided <see cref="BeatmapInfo"/>.</returns>
        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null)
        {
            if (beatmapInfo == null || beatmapInfo == DefaultBeatmap?.BeatmapInfo)
                return DefaultBeatmap;

            beatmaps.Populate(beatmapInfo);

            if (beatmapInfo.BeatmapSet == null)
                throw new InvalidOperationException($@"Beatmap set {beatmapInfo.BeatmapSetInfoID} is not in the local database.");

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapInfo.BeatmapSet.Metadata;

            WorkingBeatmap working = new BeatmapStoreWorkingBeatmap(files.Store, beatmapInfo);

            previous?.TransferTo(working);

            return working;
        }

        /// <summary>
        /// Reset the store to an empty state.
        /// </summary>
        public void Reset()
        {
            beatmaps.Reset();
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Func<BeatmapSetInfo, bool> query)
        {
            BeatmapSetInfo set = beatmaps.Query<BeatmapSetInfo>().FirstOrDefault(query);

            if (set != null)
                beatmaps.Populate(set);

            return set;
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public List<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.QueryAndPopulate(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo QueryBeatmap(Func<BeatmapInfo, bool> query)
        {
            BeatmapInfo set = beatmaps.Query<BeatmapInfo>().FirstOrDefault(query);

            if (set != null)
                beatmaps.Populate(set);

            return set;
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public List<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.QueryAndPopulate(query);

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the beatmap content.</param>
        /// <returns>A reader giving access to the beatmap's content.</returns>
        private ArchiveReader getReaderFrom(string path)
        {
            if (ZipFile.IsZipFile(path))
                return new OszArchiveReader(storage.GetStream(path));
            else
                return new LegacyFilesystemReader(path);
        }

        /// <summary>
        /// Import a beamap into our local <see cref="FileDatabase"/> storage.
        /// If the beatmap is already imported, the existing instance will be returned.
        /// </summary>
        /// <param name="reader">The beatmap archive to be read.</param>
        /// <returns>The imported beatmap, or an existing instance if it is already present.</returns>
        private BeatmapSetInfo importToStorage(ArchiveReader reader)
        {
            // for now, concatenate all .osu files in the set to create a unique hash.
            MemoryStream hashable = new MemoryStream();
            foreach (string file in reader.Filenames.Where(f => f.EndsWith(".osu")))
                using (Stream s = reader.GetStream(file))
                    s.CopyTo(hashable);

            var hash = hashable.GetMd5Hash();

            // check if this beatmap has already been imported and exit early if so.
            var beatmapSet = beatmaps.QueryAndPopulate<BeatmapSetInfo>().FirstOrDefault(b => b.Hash == hash);
            if (beatmapSet != null)
            {
                Undelete(beatmapSet);
                return beatmapSet;
            }

            List<FileInfo> fileInfos = new List<FileInfo>();

            // import files to store
            foreach (string file in reader.Filenames)
                using (Stream s = reader.GetStream(file))
                    fileInfos.Add(files.Add(s, file));

            BeatmapMetadata metadata;

            using (var stream = new StreamReader(reader.GetStream(reader.Filenames.First(f => f.EndsWith(".osu")))))
                metadata = BeatmapDecoder.GetDecoder(stream).Decode(stream).Metadata;

            beatmapSet = new BeatmapSetInfo
            {
                OnlineBeatmapSetID = metadata.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Hash = hash,
                Files = fileInfos,
                Metadata = metadata
            };

            var mapNames = reader.Filenames.Where(f => f.EndsWith(".osu"));

            foreach (var name in mapNames)
            {
                using (var raw = reader.GetStream(name))
                using (var ms = new MemoryStream()) //we need a memory stream so we can seek and shit
                using (var sr = new StreamReader(ms))
                {
                    raw.CopyTo(ms);
                    ms.Position = 0;

                    var decoder = BeatmapDecoder.GetDecoder(sr);
                    Beatmap beatmap = decoder.Decode(sr);

                    beatmap.BeatmapInfo.Path = name;
                    beatmap.BeatmapInfo.Hash = ms.GetMd5Hash();

                    // TODO: Diff beatmap metadata with set metadata and leave it here if necessary
                    beatmap.BeatmapInfo.Metadata = null;

                    // TODO: this should be done in a better place once we actually need to dynamically update it.
                    beatmap.BeatmapInfo.Ruleset = rulesets.Query<RulesetInfo>().FirstOrDefault(r => r.ID == beatmap.BeatmapInfo.RulesetID);
                    beatmap.BeatmapInfo.StarDifficulty = rulesets.Query<RulesetInfo>().FirstOrDefault(r => r.ID == beatmap.BeatmapInfo.RulesetID)?.CreateInstance()?.CreateDifficultyCalculator(beatmap)
                                                                 .Calculate() ?? 0;

                    beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);
                }
            }

            return beatmapSet;
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="populate">Whether returned objects should be pre-populated with all data.</param>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets(bool populate = true)
        {
            if (populate)
                return beatmaps.QueryAndPopulate<BeatmapSetInfo>(b => !b.DeletePending).ToList();
            else
                return beatmaps.Query<BeatmapSetInfo>(b => !b.DeletePending).ToList();
        }
    }
}
