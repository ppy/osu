using System;
using osu.Framework.OS;
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
    }
}