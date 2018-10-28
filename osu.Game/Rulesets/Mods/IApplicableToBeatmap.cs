// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that applies changes to a <see cref="Beatmap"/>.
    /// </summary>
    public interface IApplicableToBeatmap : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="Mod"/> to a <see cref="Beatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="Beatmap"/> to apply to.</param>
        void ApplyToBeatmap(IBeatmap beatmap);
    }
}
