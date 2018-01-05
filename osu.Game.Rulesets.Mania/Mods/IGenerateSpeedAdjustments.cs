// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Mods
{
    /// <summary>
    /// A type of mod which generates speed adjustments that scroll the hit objects and bar lines.
    /// </summary>
    internal interface IGenerateSpeedAdjustments
    {
        /// <summary>
        /// Applies this mod to a hit renderer.
        /// </summary>
        /// <param name="rulesetContainer">The hit renderer to apply to.</param>
        /// <param name="hitObjectTimingChanges">The per-column list of speed adjustments for hit objects.</param>
        /// <param name="barlineTimingChanges">The list of speed adjustments for bar lines.</param>
        void ApplyToRulesetContainer(ManiaRulesetContainer rulesetContainer, ref List<SpeedAdjustmentContainer>[] hitObjectTimingChanges, ref List<SpeedAdjustmentContainer> barlineTimingChanges);
    }
}
