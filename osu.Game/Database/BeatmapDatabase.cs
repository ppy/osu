using System;
using SQLite;

namespace osu.Game.Database
{
    public class BeatmapDatabase
    {
        private static SQLiteConnection Connection { get; set; }
        
        public BeatmapDatabase()
        {
            if (Connection == null)
            {
                Connection = new SQLiteConnection("beatmap.db");
                Connection.CreateTable<BeatmapMetadata>();
                Connection.CreateTable<BeatmapSet>();
                Connection.CreateTable<Beatmap>();
            }
        }
    }
}