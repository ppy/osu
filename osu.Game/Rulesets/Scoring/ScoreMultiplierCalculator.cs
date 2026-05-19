// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// Calculates the multiplier to be applied to score with a given combination of mods.
    /// </summary>
    public class ScoreMultiplierCalculator
    {
        /// <summary>
        /// Calculates the multiplier to be applied to score with the given <paramref name="mods"/>.
        /// </summary>
        public double CalculateFor(IEnumerable<Mod> mods) => 1;
    }
}
