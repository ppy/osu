using System;
using SQLite;

namespace osu.Game.Database
{
    public class BeatmapSet
    {
        [PrimaryKey]
        public int BeatmapSetID { get; set; }
        [NotNull, Indexed]
        public int MetadataID { get; set; }
    }
}

