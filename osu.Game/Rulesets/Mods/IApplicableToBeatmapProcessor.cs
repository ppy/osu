// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

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
        /// <param name="beatmapProcessor">The <see cref="BeatmapProcessor"/> to apply to.</param>
        void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor);
    }
}
