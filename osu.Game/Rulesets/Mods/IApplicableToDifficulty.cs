// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make general adjustments to difficulty.
    /// </summary>
    public interface IApplicableToDifficulty : IApplicableMod
    {
        /// <summary>
        /// Called when a beatmap is changed. Can be used to read default values.
        /// Any changes made will not be preserved.
        /// </summary>
        /// <param name="difficulty">The difficulty to read from.</param>
        void ReadFromDifficulty(BeatmapDifficulty difficulty);

        /// <summary>
        /// Called post beatmap conversion. Can be used to apply changes to difficulty attributes.
        /// </summary>
        /// <param name="difficulty">The difficulty to mutate.</param>
        void ApplyToDifficulty(BeatmapDifficulty difficulty);
    }
}
