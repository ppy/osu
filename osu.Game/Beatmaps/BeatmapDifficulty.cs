// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    public class BeatmapDifficulty : IHasPrimaryKey
    {
        /// <summary>
        /// The default value used for all difficulty settings except <see cref="SliderMultiplier"/> and <see cref="SliderTickRate"/>.
        /// </summary>
        public const float DEFAULT_DIFFICULTY = 5;

        public int ID { get; set; }

        public float DrainRate { get; set; } = DEFAULT_DIFFICULTY;
        public float CircleSize { get; set; } = DEFAULT_DIFFICULTY;
        public float OverallDifficulty { get; set; } = DEFAULT_DIFFICULTY;

        private float? approachRate;

        public float ApproachRate
        {
            get => approachRate ?? OverallDifficulty;
            set => approachRate = value;
        }

        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;

        /// <summary>
        /// Returns a shallow-clone of this <see cref="BeatmapDifficulty"/>.
        /// </summary>
        public BeatmapDifficulty Clone()
        {
            var diff = new BeatmapDifficulty();
            CopyTo(diff);
            return diff;
        }

        public void CopyTo(BeatmapDifficulty difficulty)
        {
            difficulty.ApproachRate = ApproachRate;
            difficulty.DrainRate = DrainRate;
            difficulty.CircleSize = CircleSize;
            difficulty.OverallDifficulty = OverallDifficulty;

            difficulty.SliderMultiplier = SliderMultiplier;
            difficulty.SliderTickRate = SliderTickRate;
        }

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

        /// <summary>
        /// Maps a difficulty value [0, 10] to a two-piece linear range of values.
        /// </summary>
        /// <param name="difficulty">The difficulty value to be mapped.</param>
        /// <param name="range">The values that define the two linear ranges.
        /// <list type="table">
        ///   <item>
        ///     <term>od0</term>
        ///     <description>Minimum of the resulting range which will be achieved by a difficulty value of 0.</description>
        ///   </item>
        ///   <item>
        ///     <term>od5</term>
        ///     <description>Midpoint of the resulting range which will be achieved by a difficulty value of 5.</description>
        ///   </item>
        ///   <item>
        ///     <term>od10</term>
        ///     <description>Maximum of the resulting range which will be achieved by a difficulty value of 10.</description>
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>Value to which the difficulty value maps in the specified range.</returns>
        public static double DifficultyRange(double difficulty, (double od0, double od5, double od10) range)
            => DifficultyRange(difficulty, range.od0, range.od5, range.od10);
    }
}
