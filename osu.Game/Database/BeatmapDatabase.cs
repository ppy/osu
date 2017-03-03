// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
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
    public class BeatmapDatabase
    {
        private SQLiteConnection connection { get; set; }
        private Storage storage;
        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        private BeatmapImporter ipc;

        public BeatmapDatabase(Storage storage, GameHost importHost = null)
        {
            this.storage = storage;

            if (importHost != null)
                ipc = new BeatmapImporter(importHost, this);

            if (connection == null)
            {
                try
                {
                    connection = prepareConnection();
                    deletePending();
                }
                catch (Exception e)
                {
                    Logger.Error(e, @"Failed to initialise the beatmap database! Trying again with a clean database...");
                    storage.DeleteDatabase(@"beatmaps");
                    connection = prepareConnection();
                }
            }
        }

        private void deletePending()
        {
            foreach (var b in Query<BeatmapSetInfo>().Where(b => b.DeletePending))
            {
                try
                {
                    storage.Delete(b.Path);

                    GetChildren(b, true);

                    foreach (var i in b.Beatmaps)
                    {
                        if (i.Metadata != null) connection.Delete(i.Metadata);
                        if (i.BaseDifficulty != null) connection.Delete(i.BaseDifficulty);

                        connection.Delete(i);
                    }

                    if (b.Metadata != null) connection.Delete(b.Metadata);
                    connection.Delete(b);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {b.ToString()}");
                }
            }

            //this is required because sqlite migrations don't work, initially inserting nulls into this field.
            //see https://github.com/praeclarum/sqlite-net/issues/326
            connection.Query<BeatmapSetInfo>("UPDATE BeatmapSetInfo SET DeletePending = 0 WHERE DeletePending IS NULL");
        }

        private SQLiteConnection prepareConnection()
        {
            var conn = storage.GetDatabase(@"beatmaps");

            try
            {
                conn.CreateTable<BeatmapMetadata>();
                conn.CreateTable<BaseDifficulty>();
                conn.CreateTable<BeatmapSetInfo>();
                conn.CreateTable<BeatmapInfo>();
            }
            catch
            {
                conn.Close();
                throw;
            }

            return conn;
        }

        public void Reset()
        {
            foreach (var setInfo in Query<BeatmapSetInfo>())
            {
                if (storage.Exists(setInfo.Path))
                    storage.Delete(setInfo.Path);
            }

            connection.DeleteAll<BeatmapMetadata>();
            connection.DeleteAll<BaseDifficulty>();
            connection.DeleteAll<BeatmapSetInfo>();
            connection.DeleteAll<BeatmapInfo>();
        }

        /// <summary>
        /// Import multiple <see cref="BeatmapSetInfo"/> from <paramref name="paths"/>.
        /// </summary>
        /// <param name="paths">Multiple locations on disk</param>
        public void Import(IEnumerable<string> paths)
        {
            Stack<BeatmapSetInfo> sets = new Stack<BeatmapSetInfo>();

            foreach (string p in paths)
                try
                {
                    BeatmapSetInfo set = getBeatmapSet(p);

                    //If we have an ID then we already exist in the database.
                    if (set.ID == 0)
                        sets.Push(set);

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
                    Logger.Error(e, $@"Could not import beatmap set");
                }
            
            // Batch commit with multiple sets to database
            Import(sets);
        }

        /// <summary>
        /// Import <see cref="BeatmapSetInfo"/> from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Location on disk</param>
        public void Import(string path)
        {
            Import(new [] { path });
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

            using (var reader = ArchiveReader.GetReader(storage, path))
                metadata = reader.ReadMetadata();

            if (File.Exists(path)) // Not always the case, i.e. for LegacyFilesystemReader
            {
                using (var md5 = MD5.Create())
                using (var input = storage.GetStream(path))
                {
                    hash = BitConverter.ToString(md5.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
                    input.Seek(0, SeekOrigin.Begin);
                    path = Path.Combine(@"beatmaps", hash.Remove(1), hash.Remove(2), hash);
                    if (!storage.Exists(path))
                        using (var output = storage.GetStream(path, FileAccess.Write))
                            input.CopyTo(output);
                }
            }

            var existing = connection.Table<BeatmapSetInfo>().FirstOrDefault(b => b.Hash == hash);

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

            using (var reader = ArchiveReader.GetReader(storage, path))
            {
                string[] mapNames = reader.BeatmapFilenames;
                foreach (var name in mapNames)
                    using (var stream = new StreamReader(reader.GetStream(name)))
                    {
                        var decoder = BeatmapDecoder.GetDecoder(stream);
                        Beatmap beatmap = decoder.Decode(stream);
                        beatmap.BeatmapInfo.Path = name;

                        // TODO: Diff beatmap metadata with set metadata and leave it here if necessary
                        beatmap.BeatmapInfo.Metadata = null;

                        beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);
                    }
                beatmapSet.StoryboardFile = reader.StoryboardFilename;
            }

            return beatmapSet;
        }

        public void Import(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            lock (connection)
            {
                connection.BeginTransaction();

                foreach (var s in beatmapSets)
                {
                    connection.InsertWithChildren(s, true);
                    BeatmapSetAdded?.Invoke(s);
                }

                connection.Commit();
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

            return ArchiveReader.GetReader(storage, beatmapSet.Path);
        }

        public BeatmapSetInfo GetBeatmapSet(int id)
        {
            return Query<BeatmapSetInfo>().FirstOrDefault(s => s.OnlineBeatmapSetID == id);
        }

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null, bool withStoryboard = false)
        {
            var beatmapSetInfo = Query<BeatmapSetInfo>().FirstOrDefault(s => s.ID == beatmapInfo.BeatmapSetInfoID);

            //we need metadata
            GetChildren(beatmapSetInfo);

            if (beatmapSetInfo == null)
                throw new InvalidOperationException($@"Beatmap set {beatmapInfo.BeatmapSetInfoID} is not in the local database.");

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapSetInfo.Metadata;

            WorkingBeatmap working = new DatabaseWorkingBeatmap(this, beatmapInfo, beatmapSetInfo, withStoryboard);

            previous?.TransferTo(working);

            return working;
        }

        public TableQuery<T> Query<T>() where T : class
        {
            return connection.Table<T>();
        }

        public T GetWithChildren<T>(object id) where T : class
        {
            return connection.GetWithChildren<T>(id);
        }

        public List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> filter = null, bool recursive = true)
            where T : class
        {
            return connection.GetAllWithChildren(filter, recursive);
        }

        public T GetChildren<T>(T item, bool recursive = false)
        {
            if (item == null) return default(T);

            connection.GetChildren(item, recursive);
            return item;
        }

        readonly Type[] validTypes = new[]
        {
            typeof(BeatmapSetInfo),
            typeof(BeatmapInfo),
            typeof(BeatmapMetadata),
            typeof(BaseDifficulty),
        };

        public void Update<T>(T record, bool cascade = true) where T : class
        {
            if (validTypes.All(t => t != typeof(T)))
                throw new ArgumentException(nameof(T), "Must be a type managed by BeatmapDatabase");
            if (cascade)
                connection.UpdateWithChildren(record);
            else
                connection.Update(record);
        }

        public bool Exists(BeatmapSetInfo beatmapSet) => storage.Exists(beatmapSet.Path);

        private class DatabaseWorkingBeatmap : WorkingBeatmap
        {
            private readonly BeatmapDatabase database;

            public DatabaseWorkingBeatmap(BeatmapDatabase database, BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
                : base(beatmapInfo, beatmapSetInfo, withStoryboard)
            {
                this.database = database;
            }

            protected override ArchiveReader GetReader() => database?.GetReader(BeatmapSetInfo);
        }
    }
}
