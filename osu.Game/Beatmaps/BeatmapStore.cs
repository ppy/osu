// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.IPC;
using osu.Game.Rulesets;
using SQLite.Net;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    public class BeatmapStore
    {
        // todo: make this private
        public readonly BeatmapDatabase Database;

        private readonly Storage storage;

        private readonly RulesetDatabase rulesets;

        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private BeatmapIPCChannel ipc;

        /// <summary>
        /// A default representation of a WorkingBeatmap to use when no beatmap is available.
        /// </summary>
        public WorkingBeatmap DefaultBeatmap { private get; set; }

        public BeatmapStore(Storage storage, SQLiteConnection connection, RulesetDatabase rulesets, IIpcHost importHost = null)
        {
            Database = new BeatmapDatabase(connection);
            Database.BeatmapSetAdded += s => BeatmapSetAdded?.Invoke(s);
            Database.BeatmapSetRemoved += s => BeatmapSetRemoved?.Invoke(s);

            this.storage = storage;
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
                    Import(ArchiveReader.GetReader(storage, path));

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
        public void Import(ArchiveReader archiveReader)
        {
            BeatmapSetInfo set = importToStorage(archiveReader);

            //If we have an ID then we already exist in the database.
            if (set.ID == 0)
                Database.Import(new[] { set });
        }

        /// <summary>
        /// Delete a beatmap from the store.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to delete.</param>
        public void Delete(BeatmapSetInfo beatmapSet) => Database.Delete(new[] { beatmapSet });

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null)
        {
            if (beatmapInfo == null || beatmapInfo == DefaultBeatmap?.BeatmapInfo)
                return DefaultBeatmap;

            if (beatmapInfo.BeatmapSet == null || beatmapInfo.Ruleset == null)
                beatmapInfo = Database.GetChildren(beatmapInfo, true);

            if (beatmapInfo.BeatmapSet == null)
                throw new InvalidOperationException($@"Beatmap set {beatmapInfo.BeatmapSetInfoID} is not in the local database.");

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapInfo.BeatmapSet.Metadata;

            WorkingBeatmap working = new BeatmapStoreWorkingBeatmap(() => string.IsNullOrEmpty(beatmapInfo.BeatmapSet.Path) ? null : ArchiveReader.GetReader(storage, beatmapInfo.BeatmapSet.Path), beatmapInfo);

            previous?.TransferTo(working);

            return working;
        }

        /// <summary>
        /// Reset the store to an empty state.
        /// </summary>
        public void Reset()
        {
            Database.Reset();
        }

        private BeatmapSetInfo importToStorage(ArchiveReader archiveReader)
        {
            BeatmapMetadata metadata;

            using (var stream = new StreamReader(archiveReader.GetStream(archiveReader.BeatmapFilenames[0])))
                metadata = BeatmapDecoder.GetDecoder(stream).Decode(stream).Metadata;

            string hash;
            string path;

            using (var input = archiveReader.GetUnderlyingStream())
            {
                hash = input.GetMd5Hash();
                input.Seek(0, SeekOrigin.Begin);
                path = Path.Combine(@"beatmaps", hash.Remove(1), hash.Remove(2), hash);
                if (!storage.Exists(path))
                    using (var output = storage.GetStream(path, FileAccess.Write))
                        input.CopyTo(output);
            }

            var existing = Database.Query<BeatmapSetInfo>().FirstOrDefault(b => b.Hash == hash);

            if (existing != null)
            {
                Database.GetChildren(existing);

                if (existing.DeletePending)
                {
                    existing.DeletePending = false;
                    Database.Update(existing, false);
                    BeatmapSetAdded?.Invoke(existing);
                }

                return existing;
            }

            var beatmapSet = new BeatmapSetInfo
            {
                OnlineBeatmapSetID = metadata.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Path = path,
                Hash = hash,
                Metadata = metadata
            };

            using (var archive = ArchiveReader.GetReader(storage, path))
            {
                string[] mapNames = archive.BeatmapFilenames;
                foreach (var name in mapNames)
                    using (var raw = archive.GetStream(name))
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
                        beatmap.BeatmapInfo.StarDifficulty = rulesets.Query<RulesetInfo>().FirstOrDefault(r => r.ID == beatmap.BeatmapInfo.RulesetID)?.CreateInstance()?.CreateDifficultyCalculator(beatmap).Calculate() ?? 0;

                        beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);
                    }
                beatmapSet.StoryboardFile = archive.StoryboardFilename;
            }

            return beatmapSet;
        }
    }
}
