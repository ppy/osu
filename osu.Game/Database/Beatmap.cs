using System;
using SQLite;

namespace osu.Game.Database
{
    public class Beatmap
    {
        [PrimaryKey]
        public int ID { get; set; }
        [NotNull, Indexed]
        public int BeatmapSetID { get; set; }
        [Indexed]
        public int BeatmapMetadataID { get; set; }
        public float DrainRate { get; set; }
        public float CircleSize { get; set; }
        public float OverallDifficulty { get; set; }
        public float ApproachRate { get; set; }
        public float SliderMultiplier { get; set; }
        public float SliderTickRate { get; set; }
    }
}