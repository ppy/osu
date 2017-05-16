// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using osu.Game.IPC;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;

namespace osu.Game.Database
{
    public class BeatmapDatabase : Database
    {
        private readonly RulesetDatabase rulesets;

        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private BeatmapIPCChannel ipc;

        public BeatmapDatabase(Storage storage, SQLiteConnection connection, RulesetDatabase rulesets, IIpcHost importHost = null) : base(storage, connection)
        {
            this.rulesets = rulesets;
            if (importHost != null)
                ipc = new BeatmapIPCChannel(importHost, this);
        }

        private void deletePending()
        {
            foreach (var b in GetAllWithChildren<BeatmapSetInfo>(b => b.DeletePending))
            {
                try
                {
                    Storage.Delete(b.Path);

                    foreach (var i in b.Beatmaps)
                    {
                        if (i.Metadata != null) Connection.Delete(i.Metadata);
                        if (i.Difficulty != null) Connection.Delete(i.Difficulty);

                        Connection.Delete(i);
                    }

                    if (b.Metadata != null) Connection.Delete(b.Metadata);
                    Connection.Delete(b);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {b}");
                }
            }

            //this is required because sqlite migrations don't work, initially inserting nulls into this field.
            //see https://github.com/praeclarum/sqlite-net/issues/326
            Connection.Query<BeatmapSetInfo>("UPDATE BeatmapSetInfo SET DeletePending = 0 WHERE DeletePending IS NULL");
        }

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<BeatmapMetadata>();
            Connection.CreateTable<BeatmapDifficulty>();
            Connection.CreateTable<BeatmapSetInfo>();
            Connection.CreateTable<BeatmapInfo>();

            if (reset)
            {
                Storage.DeleteDatabase(@"beatmaps");

                foreach (var setInfo in Query<BeatmapSetInfo>())
                {
                    if (Storage.Exists(setInfo.Path))
                        Storage.Delete(setInfo.Path);
                }

                Connection.DeleteAll<BeatmapMetadata>();
                Connection.DeleteAll<BeatmapDifficulty>();
                Connection.DeleteAll<BeatmapSetInfo>();
                Connection.DeleteAll<BeatmapInfo>();
            }

            deletePending();
        }

        protected override Type[] ValidTypes => new[] {
            typeof(BeatmapSetInfo),
            typeof(BeatmapInfo),
            typeof(BeatmapMetadata),
            typeof(BeatmapDifficulty),
        };

        /// <summary>
        /// Import multiple <see cref="BeatmapSetInfo"/> from <paramref name="paths"/>.
        /// </summary>
        /// <param name="paths">Multiple locations on disk</param>
        public void Import(IEnumerable<string> paths)
        {
            foreach (string p in paths)
            {
                try
                {
                    BeatmapSetInfo set = getBeatmapSet(p);

                    //If we have an ID then we already exist in the database.
                    if (set.ID == 0)
                        Import(new[] { set });

                    // We may or may not want to delete the file depending on where it is stored.
                    //  e.g. reconstructing/repairing database with beatmaps from default storage.
                    // Also, not always a single file, i.e. for LegacyFilesystemReader
                    // TODO: Add a check to prevent files from storage to be deleted.
                    try
                    {
                        File.Delete(p);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete file at {p}");
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
        /// Import <see cref="BeatmapSetInfo"/> from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Location on disk</param>
        public void Import(string path)
        {
            Import(new[] { path });
        }

        /// <summary>
        /// Duplicates content from <paramref name="path"/> to storage and returns a representing <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="path">Content location</param>
        /// <returns><see cref="BeatmapSetInfo"/></returns>
        private BeatmapSetInfo getBeatmapSet(string path)
        {
            string hash = null;

            BeatmapMetadata metadata;

            using (var reader = ArchiveReader.GetReader(Storage, path))
            {
                using (var stream = new StreamReader(reader.GetStream(reader.BeatmapFilenames[0])))
                    metadata = BeatmapDecoder.GetDecoder(stream).Decode(stream).Metadata;
            }

            if (File.Exists(path)) // Not always the case, i.e. for LegacyFilesystemReader
            {
                using (var input = Storage.GetStream(path))
                {
                    hash = input.GetMd5Hash();
                    input.Seek(0, SeekOrigin.Begin);
                    path = Path.Combine(@"beatmaps", hash.Remove(1), hash.Remove(2), hash);
                    if (!Storage.Exists(path))
                        using (var output = Storage.GetStream(path, FileAccess.Write))
                            input.CopyTo(output);
                }
            }

            var existing = Connection.Table<BeatmapSetInfo>().FirstOrDefault(b => b.Hash == hash);

            if (existing != null)
            {
                if (existing.DeletePending)
                {
                    existing.DeletePending = false;
                    Update(existing, false);
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

            using (var archive = ArchiveReader.GetReader(Storage, path))
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

        public void Import(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            lock (Connection)
            {
                Connection.BeginTransaction();

                foreach (var s in beatmapSets)
                {
                    Connection.InsertOrReplaceWithChildren(s, true);
                    BeatmapSetAdded?.Invoke(s);
                }

                Connection.Commit();
            }
        }

        public void Delete(BeatmapSetInfo beatmapSet)
        {
            beatmapSet.DeletePending = true;
            Update(beatmapSet, false);

            BeatmapSetRemoved?.Invoke(beatmapSet);
        }

        public ArchiveReader GetReader(BeatmapSetInfo beatmapSet)
        {
            if (string.IsNullOrEmpty(beatmapSet.Path))
                return null;

            return ArchiveReader.GetReader(Storage, beatmapSet.Path);
        }

        public BeatmapSetInfo GetBeatmapSet(int id)
        {
            return Query<BeatmapSetInfo>().FirstOrDefault(s => s.OnlineBeatmapSetID == id);
        }

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null, bool withStoryboard = false)
        {
            if (beatmapInfo.BeatmapSet == null || beatmapInfo.Ruleset == null)
                beatmapInfo = GetChildren(beatmapInfo, true);

            if (beatmapInfo.BeatmapSet == null)
                throw new InvalidOperationException($@"Beatmap set {beatmapInfo.BeatmapSetInfoID} is not in the local database.");

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapInfo.BeatmapSet.Metadata;

            WorkingBeatmap working = new DatabaseWorkingBeatmap(this, beatmapInfo, withStoryboard);

            previous?.TransferTo(working);

            return working;
        }

        public bool Exists(BeatmapSetInfo beatmapSet) => Storage.Exists(beatmapSet.Path);
    }
}
