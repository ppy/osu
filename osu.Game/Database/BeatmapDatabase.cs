//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
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
        private BasicStorage storage;
        public event Action<BeatmapSetInfo> BeatmapSetAdded;

        private BeatmapImporter ipc;

        public BeatmapDatabase(BasicStorage storage, BasicGameHost importHost = null)
        {
            this.storage = storage;

            if (importHost != null)
                ipc = new BeatmapImporter(importHost, this);

            if (connection == null)
            {
                connection = storage.GetDatabase(@"beatmaps");
                connection.CreateTable<BeatmapMetadata>();
                connection.CreateTable<BaseDifficulty>();
                connection.CreateTable<BeatmapSetInfo>();
                connection.CreateTable<BeatmapInfo>();
            }
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

        public void Import(params string[] paths)
        {
            foreach (string p in paths)
            {
                var path = p;
                string hash = null;

                BeatmapMetadata metadata;

                using (var reader = ArchiveReader.GetReader(storage, path))
                    metadata = reader.ReadMetadata();

                if (metadata.OnlineBeatmapSetID.HasValue &&
                    connection.Table<BeatmapSetInfo>().Count(b => b.OnlineBeatmapSetID == metadata.OnlineBeatmapSetID) != 0)
                    return; // TODO: Update this beatmap instead

                if (File.Exists(path)) // Not always the case, i.e. for LegacyFilesystemReader
                {
                    using (var md5 = MD5.Create())
                    using (var input = storage.GetStream(path))
                    {
                        hash = BitConverter.ToString(md5.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
                        input.Seek(0, SeekOrigin.Begin);
                        path = Path.Combine(@"beatmaps", hash.Remove(1), hash.Remove(2), hash);
                        using (var output = storage.GetStream(path, FileAccess.Write))
                            input.CopyTo(output);
                    }
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
                    string[] mapNames = reader.ReadBeatmaps();
                    foreach (var name in mapNames)
                    {
                        using (var stream = new StreamReader(reader.GetStream(name)))
                        {
                            var decoder = BeatmapDecoder.GetDecoder(stream);
                            Beatmap beatmap = decoder.Decode(stream);
                            beatmap.BeatmapInfo.Path = name;

                            // TODO: Diff beatmap metadata with set metadata and leave it here if necessary
                            beatmap.BeatmapInfo.Metadata = null;

                            beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);
                        }
                    }
                }

                Import(new[] { beatmapSet });
            }
        }

        public void Import(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            connection.BeginTransaction();

            foreach (var s in beatmapSets)
            {
                connection.InsertWithChildren(s, true);
                BeatmapSetAdded?.Invoke(s);
            }

            connection.Commit();
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

        public WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo, WorkingBeatmap previous = null)
        {
            var beatmapSetInfo = Query<BeatmapSetInfo>().FirstOrDefault(s => s.ID == beatmapInfo.BeatmapSetInfoID);

            //we need metadata
            GetChildren(beatmapSetInfo);

            if (beatmapSetInfo == null)
                throw new InvalidOperationException($@"Beatmap set {beatmapInfo.BeatmapSetInfoID} is not in the local database.");

            if (beatmapInfo.Metadata == null)
                beatmapInfo.Metadata = beatmapSetInfo.Metadata;

            var working = new WorkingBeatmap(beatmapInfo, beatmapSetInfo, this);

            previous?.TransferTo(working);

            return working;
        }

        public Beatmap GetBeatmap(BeatmapInfo beatmapInfo)
        {
            using (WorkingBeatmap data = GetWorkingBeatmap(beatmapInfo))
                return data.Beatmap;
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
    }
}
