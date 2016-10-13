using System;
using System.Collections.Generic;
using System.IO;
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
        
        public BeatmapDatabase(BasicStorage storage)
        {
            if (connection == null)
            {
                connection = storage.GetDatabase(@"beatmaps");
                connection.CreateTable<BeatmapMetadata>();
                connection.CreateTable<BaseDifficulty>();
                connection.CreateTable<BeatmapSet>();
                connection.CreateTable<Beatmap>();
            }
        }

        public void AddBeatmap(ArchiveReader input)
        {
            var metadata = input.ReadMetadata();
            if (connection.Table<BeatmapSet>().Count(b => b.BeatmapSetID == metadata.BeatmapSetID) != 0)
                return;
            string[] mapNames = input.ReadBeatmaps();
            var beatmapSet = new BeatmapSet { BeatmapSetID = metadata.BeatmapSetID };
            var maps = new List<Beatmap>();
            foreach (var name in mapNames)
            {
                using (var stream = new StreamReader(input.ReadFile(name)))
                {
                    var decoder = BeatmapDecoder.GetDecoder(stream);
                    var beatmap = decoder.Decode(stream);
                    maps.Add(beatmap);
                    beatmap.BaseDifficultyID = connection.Insert(beatmap.BaseDifficulty);
                }
            }
            beatmapSet.BeatmapMetadataID = connection.Insert(metadata);
            connection.Insert(beatmapSet);
            connection.InsertAll(maps);
        }
    }
}