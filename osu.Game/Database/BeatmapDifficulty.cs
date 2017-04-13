// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using SQLite.Net.Attributes;

namespace osu.Game.Database
{
    public class BeatmapDifficulty
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public float DrainRate { get; set; } = 5;
        public float CircleSize { get; set; } = 5;
        public float OverallDifficulty { get; set; } = 5;
        public float ApproachRate { get; set; } = 5;
        public float SliderMultiplier { get; set; } = 1;
        public float SliderTickRate { get; set; } = 1;

        /// <summary>
        /// Maps a difficulty value [0, 10] to a two-piece linear range of values.
        /// </summary>
        /// <param name="difficulty">The difficulty value to be mapped.</param>
        /// <param name="min">Minimum of the resulting range which will be achieved by a difficulty value of 0.</param>
        /// <param name="mid">Midpoint of the resulting range which will be achieved by a difficulty value of 5.</param>
        /// <param name="max">Maximum of the resulting range which will be achieved by a difficulty value of 10.</param>
        /// <returns>Value to which the difficulty value maps in the specified range.</returns>
        public static double DifficultyRange(double difficulty, double min, double mid, double max)
        {
            if (difficulty > 5)
                return mid + (max - mid) * (difficulty - 5) / 5;
            if (difficulty < 5)
                return mid - (mid - min) * (5 - difficulty) / 5;
            return mid;
        }
    }
}

