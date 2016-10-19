using System;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Database
{
    public class BeatmapSetInfo
    {
        [PrimaryKey]
        public int BeatmapSetID { get; set; }
        [OneToOne]
        public BeatmapMetadata Metadata { get; set; }
        [NotNull, ForeignKey(typeof(BeatmapMetadata))]
        public int BeatmapMetadataID { get; set; }
        public string Hash { get; set; }
        public string Path { get; set; }
    }
}

