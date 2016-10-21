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
using SQLite.Net;
using SQLiteNetExtensions.Extensions;

namespace osu.Game.Database
{
    public class BeatmapDatabase
    {
        private static SQLiteConnection connection { get; set; }
        private BasicStorage storage;
        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        
        public BeatmapDatabase(BasicStorage storage)
        {
            this.storage = storage;
            if (connection == null)
            {
                connection = storage.GetDatabase(@"beatmaps");
                connection.CreateTable<BeatmapMetadata>();
                connection.CreateTable<BaseDifficulty>();
                connection.CreateTable<BeatmapSetInfo>();
                connection.CreateTable<BeatmapInfo>();
            }
        }

        public void ImportBeatmap(string path)
        {
            string hash = null;
            BeatmapMetadata metadata;

            using (var reader = ArchiveReader.GetReader(storage, path))
                metadata = reader.ReadMetadata();

            if (connection.Table<BeatmapSetInfo>().Count(b => b.BeatmapSetID == metadata.BeatmapSetID) != 0)
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
                BeatmapSetID = metadata.BeatmapSetID,
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
                    using (var stream = new StreamReader(reader.ReadFile(name)))
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
            connection.InsertWithChildren(beatmapSet, true);
            BeatmapSetAdded?.Invoke(beatmapSet);
        }

        public ArchiveReader GetReader(BeatmapSetInfo beatmapSet)
        {
            return ArchiveReader.GetReader(storage, beatmapSet.Path);
        }
        
        public BeatmapSetInfo GetBeatmapSet(int id)
        {
            return Query<BeatmapSetInfo>().Where(s => s.BeatmapSetID == id).FirstOrDefault();
        }
        
        public Beatmap GetBeatmap(BeatmapInfo beatmapInfo)
        {
            var beatmapSet = Query<BeatmapSetInfo>()
                .Where(s => s.BeatmapSetID == beatmapInfo.BeatmapSetID).FirstOrDefault();    
            if (beatmapSet == null)
                throw new InvalidOperationException(
                    $@"Beatmap set {beatmapInfo.BeatmapSetID} is not in the local database.");
            using (var reader = GetReader(beatmapSet))
            using (var stream = new StreamReader(reader.ReadFile(beatmapInfo.Path)))
            {
                var decoder = BeatmapDecoder.GetDecoder(stream);
                return decoder.Decode(stream);
            }
        }
        
        public TableQuery<T> Query<T>() where T : class
        {
            return connection.Table<T>();
        }
        
        public T GetWithChildren<T>(object id) where T : class
        {
            return connection.GetWithChildren<T>(id);
        }
        
        public List<T> GetAllWithChildren<T>(Expression<Func<T, bool>> filter = null,
            bool recursive = true) where T : class
        {
            return connection.GetAllWithChildren<T>(filter, recursive);
        }
        
        public void GetChildren<T>(T item, bool recursive = true)
        {
            connection.GetChildren<T>(item, recursive);
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
            if (!validTypes.Any(t => t == typeof(T)))
                throw new ArgumentException(nameof(T), "Must be a type managed by BeatmapDatabase");
            if (cascade)
                connection.UpdateWithChildren(record);
            else
                connection.Update(record);
        }
    }
}
