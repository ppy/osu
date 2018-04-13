// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make general adjustments to difficulty.
    /// </summary>
    public interface IApplicableToDifficulty : IApplicableMod
    {
        void ApplyToDifficulty(BeatmapDifficulty difficulty);
    }
}
