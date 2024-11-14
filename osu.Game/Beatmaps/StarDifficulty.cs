// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Beatmaps
{
    public readonly struct StarDifficulty
    {
        /// <summary>
        /// The star difficulty rating for the given beatmap.
        /// </summary>
        public readonly double Stars;

        /// <summary>
        /// The maximum combo achievable on the given beatmap.
        /// </summary>
        public readonly int MaxCombo;

        /// <summary>
        /// The difficulty attributes computed for the given beatmap.
        /// Might not be available if the star difficulty is associated with a beatmap that's not locally available.
        /// </summary>
        public readonly DifficultyAttributes? DifficultyAttributes;

        /// <summary>
        /// The performance attributes computed for a perfect score on the given beatmap.
        /// Might not be available if the star difficulty is associated with a beatmap that's not locally available.
        /// </summary>
        public readonly PerformanceAttributes? PerformanceAttributes;

        /// <summary>
        /// Creates a <see cref="StarDifficulty"/> structure.
        /// </summary>
        public StarDifficulty(DifficultyAttributes difficulty, PerformanceAttributes performance)
        {
            Stars = double.IsFinite(difficulty.StarRating) ? difficulty.StarRating : 0;
            MaxCombo = difficulty.MaxCombo;
            DifficultyAttributes = difficulty;
            PerformanceAttributes = performance;
            // Todo: Add more members (BeatmapInfo.DifficultyRating? Attributes? Etc...)
        }

        /// <summary>
        /// Creates a <see cref="StarDifficulty"/> structure with a pre-populated star difficulty and max combo
        /// in scenarios where computing <see cref="Rulesets.Difficulty.DifficultyAttributes"/> is not feasible (i.e. when working with online sources).
        /// </summary>
        public StarDifficulty(double starDifficulty, int maxCombo)
        {
            Stars = double.IsFinite(starDifficulty) ? starDifficulty : 0;
            MaxCombo = maxCombo;
        }

        public DifficultyRating DifficultyRating => GetDifficultyRating(Stars);

        /// <summary>
        /// Retrieves the <see cref="DifficultyRating"/> that describes a star rating.
        /// </summary>
        /// <remarks>
        /// For more information, see: https://osu.ppy.sh/help/wiki/Difficulties
        /// </remarks>
        /// <param name="starRating">The star rating.</param>
        /// <returns>The <see cref="DifficultyRating"/> that best describes <paramref name="starRating"/>.</returns>
        public static DifficultyRating GetDifficultyRating(double starRating)
        {
            if (Precision.AlmostBigger(starRating, 6.5, 0.005))
                return DifficultyRating.ExpertPlus;

            if (Precision.AlmostBigger(starRating, 5.3, 0.005))
                return DifficultyRating.Expert;

            if (Precision.AlmostBigger(starRating, 4.0, 0.005))
                return DifficultyRating.Insane;

            if (Precision.AlmostBigger(starRating, 2.7, 0.005))
                return DifficultyRating.Hard;

            if (Precision.AlmostBigger(starRating, 2.0, 0.005))
                return DifficultyRating.Normal;

            return DifficultyRating.Easy;
        }
    }
}
