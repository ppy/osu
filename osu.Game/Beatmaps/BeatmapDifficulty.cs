// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Realms;

namespace osu.Game.Beatmaps
{
    [MapTo("BeatmapDifficulty")]
    public class BeatmapDifficulty : EmbeddedObject, IBeatmapDifficultyInfo
    {
        /// <summary>
        /// The default value used for all difficulty settings except <see cref="SliderMultiplier"/> and <see cref="SliderTickRate"/>.
        /// </summary>
        public const float DEFAULT_DIFFICULTY = 5;

        public float DrainRate { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float CircleSize { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float OverallDifficulty { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float ApproachRate { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;

        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;

        public BeatmapDifficulty()
        {
        }

        public BeatmapDifficulty(IBeatmapDifficultyInfo source)
        {
            CopyFrom(source);
        }

        /// <summary>
        /// Returns a shallow-clone of this <see cref="BeatmapDifficulty"/>.
        /// </summary>
        public virtual BeatmapDifficulty Clone() => new BeatmapDifficulty(this);

        public virtual void CopyTo(BeatmapDifficulty difficulty)
        {
            difficulty.ApproachRate = ApproachRate;
            difficulty.DrainRate = DrainRate;
            difficulty.CircleSize = CircleSize;
            difficulty.OverallDifficulty = OverallDifficulty;

            difficulty.SliderMultiplier = SliderMultiplier;
            difficulty.SliderTickRate = SliderTickRate;
        }

        public virtual void CopyFrom(IBeatmapDifficultyInfo other)
        {
            ApproachRate = other.ApproachRate;
            DrainRate = other.DrainRate;
            CircleSize = other.CircleSize;
            OverallDifficulty = other.OverallDifficulty;

            SliderMultiplier = other.SliderMultiplier;
            SliderTickRate = other.SliderTickRate;
        }
    }
}
