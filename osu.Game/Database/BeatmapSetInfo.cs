using System;
using System.Collections.Generic;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Database
{
    public class BeatmapSetInfo
    {
        [PrimaryKey]
        public int BeatmapSetID { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BeatmapMetadata Metadata { get; set; }

        [NotNull, ForeignKey(typeof(BeatmapMetadata))]
        public int BeatmapMetadataID { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<BeatmapInfo> Beatmaps { get; set; }

        public string Hash { get; set; }

        public string Path { get; set; }
    }
}

