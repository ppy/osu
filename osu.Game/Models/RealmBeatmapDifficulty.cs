// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;
using osu.Game.Beatmaps;
using Realms;

#nullable enable

namespace osu.Game.Models
{
    [ExcludeFromDynamicCompile]
    [MapTo("BeatmapDifficulty")]
    public class RealmBeatmapDifficulty : EmbeddedObject, IBeatmapDifficultyInfo
    {
        public float DrainRate { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float CircleSize { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float OverallDifficulty { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;
        public float ApproachRate { get; set; } = IBeatmapDifficultyInfo.DEFAULT_DIFFICULTY;

        public double SliderMultiplier { get; set; } = 1;
        public double SliderTickRate { get; set; } = 1;

        /// <summary>
        /// Returns a shallow-clone of this <see cref="RealmBeatmapDifficulty"/>.
        /// </summary>
        public RealmBeatmapDifficulty Clone()
        {
            var diff = new RealmBeatmapDifficulty();
            CopyTo(diff);
            return diff;
        }

        public void CopyTo(RealmBeatmapDifficulty difficulty)
        {
            difficulty.ApproachRate = ApproachRate;
            difficulty.DrainRate = DrainRate;
            difficulty.CircleSize = CircleSize;
            difficulty.OverallDifficulty = OverallDifficulty;

            difficulty.SliderMultiplier = SliderMultiplier;
            difficulty.SliderTickRate = SliderTickRate;
        }
    }
}
