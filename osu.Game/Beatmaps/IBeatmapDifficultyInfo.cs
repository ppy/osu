// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A representation of all top-level difficulty settings for a beatmap.
    /// </summary>
    public interface IBeatmapDifficultyInfo
    {
        /// <summary>
        /// The default value used for all difficulty settings except <see cref="SliderMultiplier"/> and <see cref="SliderTickRate"/>.
        /// </summary>
        const float DEFAULT_DIFFICULTY = 5;

        /// <summary>
        /// The drain rate of the associated beatmap.
        /// </summary>
        float DrainRate { get; }

        /// <summary>
        /// The circle size of the associated beatmap.
        /// </summary>
        float CircleSize { get; }

        /// <summary>
        /// The overall difficulty of the associated beatmap.
        /// </summary>
        float OverallDifficulty { get; }

        /// <summary>
        /// The approach rate of the associated beatmap.
        /// </summary>
        float ApproachRate { get; }

        /// <summary>
        /// The base slider velocity of the associated beatmap.
        /// This was known as "SliderMultiplier" in the .osu format and stable editor.
        /// </summary>
        double SliderMultiplier { get; }

        /// <summary>
        /// The slider tick rate of the associated beatmap.
        /// </summary>
        double SliderTickRate { get; }

        /// <summary>
        /// The number of hitobjects in the beatmap with a distinct end time.
        /// </summary>
        /// <remarks>
        /// Canonically, these are hitobjects are either sliders or spinners.
        /// </remarks>
        int EndTimeObjectCount { get; }

        /// <summary>
        /// The total number of hitobjects in the beatmap.
        /// </summary>
        int TotalObjectCount { get; }

        /// <summary>
        /// Maps a difficulty value [0, 10] to a two-piece linear range of values.
        /// </summary>
        /// <param name="difficulty">The difficulty value to be mapped.</param>
        /// <param name="min">Minimum of the resulting range which will be achieved by a difficulty value of 0.</param>
        /// <param name="mid">Midpoint of the resulting range which will be achieved by a difficulty value of 5.</param>
        /// <param name="max">Maximum of the resulting range which will be achieved by a difficulty value of 10.</param>
        /// <returns>Value to which the difficulty value maps in the specified range.</returns>
        static double DifficultyRange(double difficulty, double min, double mid, double max)
        {
            if (difficulty > 5)
                return mid + (max - mid) * DifficultyRange(difficulty);
            if (difficulty < 5)
                return mid + (mid - min) * DifficultyRange(difficulty);

            return mid;
        }

        /// <summary>
        /// Maps a difficulty value [0, 10] to a linear range of [-1, 1].
        /// </summary>
        /// <param name="difficulty">The difficulty value to be mapped.</param>
        /// <returns>Value to which the difficulty value maps in the specified range.</returns>
        static double DifficultyRange(double difficulty) => (difficulty - 5) / 5;

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
        static double DifficultyRange(double difficulty, (double od0, double od5, double od10) range)
            => DifficultyRange(difficulty, range.od0, range.od5, range.od10);
    }
}
