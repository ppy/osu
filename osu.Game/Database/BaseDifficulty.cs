//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using SQLite.Net.Attributes;

namespace osu.Game.Database
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

