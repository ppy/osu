using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using SQLite;

namespace osu.Game.Database
{
    public class BeatmapDatabase
    {
        private static SQLiteConnection connection { get; set; }
        private BasicStorage storage;
        
        public BeatmapDatabase(BasicStorage storage)
        {
            this.storage = storage;
            if (connection == null)
            {
                connection = storage.GetDatabase(@"beatmaps");
                connection.CreateTable<BeatmapMetadata>();
                connection.CreateTable<BaseDifficulty>();
                connection.CreateTable<BeatmapSet>();
                connection.CreateTable<Beatmap>();
            }
        }
        public void AddBeatmap(string path)
        {
            string hash = null;
            ArchiveReader reader;
            if (File.Exists(path)) // Not always the case, i.e. for LegacyFilesystemReader
            {
                using (var md5 = MD5.Create())
                using (var input = storage.GetStream(path))
                {
                    hash = BitConverter.ToString(md5.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
                    input.Seek(0, SeekOrigin.Begin);
                    var outputPath = Path.Combine(@"beatmaps", hash.Remove(1), hash.Remove(2), hash);
                    using (var output = storage.GetStream(outputPath, FileAccess.Write))
                        input.CopyTo(output);
                    reader = ArchiveReader.GetReader(storage, path = outputPath);
                }
            }
            else
                reader = ArchiveReader.GetReader(storage, path);
            var metadata = reader.ReadMetadata();
            if (connection.Table<BeatmapSet>().Count(b => b.BeatmapSetID == metadata.BeatmapSetID) != 0)
                return; // TODO: Update this beatmap instead
            string[] mapNames = reader.ReadBeatmaps();
            var beatmapSet = new BeatmapSet
            {
                BeatmapSetID = metadata.BeatmapSetID,
                Path = path,
                Hash = hash,
            };
            var maps = new List<Beatmap>();
            foreach (var name in mapNames)
            {
                using (var stream = new StreamReader(reader.ReadFile(name)))
                {
                    var decoder = BeatmapDecoder.GetDecoder(stream);
                    Beatmap beatmap = new Beatmap();
                    decoder.Decode(stream, beatmap);
                    maps.Add(beatmap);
                    beatmap.BaseDifficultyID = connection.Insert(beatmap.BaseDifficulty);
                }
            }
            beatmapSet.BeatmapMetadataID = connection.Insert(metadata);
            connection.Insert(beatmapSet);
            connection.InsertAll(maps);
        }
        public ArchiveReader GetReader(BeatmapSet beatmapSet)
        {
            return ArchiveReader.GetReader(storage, beatmapSet.Path);
        }

        /// <summary>
        /// Given a BeatmapSet pulled from the database, loads the rest of its data from disk.
        /// </summary>        public void PopulateBeatmap(BeatmapSet beatmapSet)
        {
            using (var reader = GetReader(beatmapSet))
            {
                string[] mapNames = reader.ReadBeatmaps();
                foreach (var name in mapNames)
                {
                    using (var stream = new StreamReader(reader.ReadFile(name)))
                    {
                        var decoder = BeatmapDecoder.GetDecoder(stream);
                        Beatmap beatmap = new Beatmap();
                        decoder.Decode(stream, beatmap);
                        beatmapSet.Beatmaps.Add(beatmap);
                    }
                }
            }
        }
    }
}