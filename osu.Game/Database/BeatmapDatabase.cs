using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.OS;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.IO;
using SQLite;

namespace osu.Game.Database
{
    public class BeatmapDatabase
    {
        private static SQLiteConnection Connection { get; set; }
        
        public BeatmapDatabase(BasicStorage storage)
        {
            if (Connection == null)
            {
                Connection = storage.GetDatabase(@"beatmaps");
                Connection.CreateTable<BeatmapMetadata>();
                Connection.CreateTable<BeatmapSet>();
                Connection.CreateTable<Beatmap>();
            }
        }
        public void AddBeatmap(ArchiveReader input)
        {
            var metadata = input.ReadMetadata();
            if (Connection.Table<BeatmapSet>().Count(b => b.BeatmapSetID == metadata.BeatmapSetID) != 0)
                return;
            string[] mapNames = input.ReadBeatmaps();
            var beatmapSet = new BeatmapSet { BeatmapSetID = metadata.BeatmapSetID };
            var beatmapMetadata = new BeatmapMetadata
            {
                Title = metadata.Title,
                TitleUnicode = metadata.TitleUnicode,
                Artist = metadata.Artist,
                ArtistUnicode = metadata.ArtistUnicode,
                Author = metadata.Author,
                Source = metadata.Source,
                Tags = metadata.Tags,
                Mode = metadata.Mode,
                PreviewTime = metadata.PreviewTime,
                AudioFile = metadata.AudioFile,
                BackgroundFile = metadata.BackgroundFile,
            };
            var maps = new List<Beatmap>();
            foreach (var name in mapNames)
            {
                using (var stream = new StreamReader(input.ReadFile(name)))
                {
                    var decoder = BeatmapDecoder.GetDecoder(stream);
                    var beatmap = decoder.Decode(stream);
                    maps.Add(new Beatmap
                    {
                        BeatmapID = beatmap.BeatmapID,
                        BeatmapSetID = metadata.BeatmapSetID,
                        // TODO: Import more things
                    });
                }
            }
            beatmapSet.BeatmapMetadataID = Connection.Insert(beatmapMetadata);
            Connection.Insert(beatmapSet);
            Connection.InsertAll(maps);
        }
    }
}