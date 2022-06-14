// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    [Table(@"BeatmapDifficulty")]
    public class EFBeatmapDifficulty : IHasPrimaryKey, IBeatmapDifficultyInfo
    {
        /// <summary>
        /// The default value used for all difficulty settings except <see cref="SliderMultiplier"/> and <see cref="SliderTickRate"/>.
        /// </summary>
        public const float DEFAULT_DIFFICULTY = 5;

        public int ID { get; set; }

        public bool IsManaged => ID > 0;

        public float DrainRate { get; set; } = DEFAULT_DIFFICULTY;
        public float CircleSize { get; set; } = DEFAULT_DIFFICULTY;
        public float OverallDifficulty { get; set; } = DEFAULT_DIFFICULTY;

        private float? approachRate;

        public EFBeatmapDifficulty()
        {
        }

        public EFBeatmapDifficulty(IBeatmapDifficultyInfo source)
        {
            CopyFrom(source);
        }

        public float ApproachRate
        {
            get => approachRate ?? OverallDifficulty;
            set => approachRate = value;
        }

        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;

        /// <summary>
        /// Returns a shallow-clone of this <see cref="EFBeatmapDifficulty"/>.
        /// </summary>
        public EFBeatmapDifficulty Clone()
        {
            var diff = (EFBeatmapDifficulty)Activator.CreateInstance(GetType());
            CopyTo(diff);
            return diff;
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

        public virtual void CopyTo(EFBeatmapDifficulty other)
        {
            other.ApproachRate = ApproachRate;
            other.DrainRate = DrainRate;
            other.CircleSize = CircleSize;
            other.OverallDifficulty = OverallDifficulty;

            other.SliderMultiplier = SliderMultiplier;
            other.SliderTickRate = SliderTickRate;
        }
    }
}
