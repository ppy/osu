// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="BeatmapProcessor"/>.
    /// </summary>
    public interface IApplicableToBeatmapProcessor : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="Mod"/> to a <see cref="BeatmapProcessor"/>.
        /// </summary>
        void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor);
    }
}
