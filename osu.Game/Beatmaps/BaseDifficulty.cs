using System;
using SQLite;

namespace osu.Game.Beatmaps
{
    public class BaseDifficulty
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public float DrainRate { get; set; }
        public float CircleSize { get; set; }
        public float OverallDifficulty { get; set; }
        public float ApproachRate { get; set; }
        public float SliderMultiplier { get; set; }
        public float SliderTickRate { get; set; }
    }
}

