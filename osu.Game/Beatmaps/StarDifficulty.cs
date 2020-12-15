// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
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
        [CanBeNull]
        public readonly DifficultyAttributes Attributes;

        /// <summary>
        /// Creates a <see cref="StarDifficulty"/> structure based on <see cref="DifficultyAttributes"/> computed
        /// by a <see cref="DifficultyCalculator"/>.
        /// </summary>
        public StarDifficulty([NotNull] DifficultyAttributes attributes)
        {
            Stars = attributes.StarRating;
            MaxCombo = attributes.MaxCombo;
            Attributes = attributes;
            // Todo: Add more members (BeatmapInfo.DifficultyRating? Attributes? Etc...)
        }

        /// <summary>
        /// Creates a <see cref="StarDifficulty"/> structure with a pre-populated star difficulty and max combo
        /// in scenarios where computing <see cref="DifficultyAttributes"/> is not feasible (i.e. when working with online sources).
        /// </summary>
        public StarDifficulty(double starDifficulty, int maxCombo)
        {
            Stars = starDifficulty;
            MaxCombo = maxCombo;
            Attributes = null;
        }

        public DifficultyRating DifficultyRating => BeatmapDifficultyCache.GetDifficultyRating(Stars);
    }
}
