// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="Beatmap"/> after conversion and post-processing has completed.
    /// </summary>
    public interface IApplicableToBeatmap : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToBeatmap"/> to an <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to apply to.</param>
        void ApplyToBeatmap(IBeatmap beatmap);
    }
}
