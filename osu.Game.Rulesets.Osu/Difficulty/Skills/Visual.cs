// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to read every object in the map.
    /// </summary>
    public class Visual : OsuStrainSkill
    {
        protected override double StarsPerDouble => 1.025;
        private readonly bool hasHiddenMod;
        private readonly double greatWindow;

        private double skillMultiplier => 10;
        private double strainDecayBase => 0.1;
        private double currentStrain;

        public Visual(Mod[] mods, double hitWindowGreat)
            : base(mods)
        {
            hasHiddenMod = mods.Any(m => m is OsuModHidden);
            greatWindow = hitWindowGreat;
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += VisualEvaluator.EvaluateDifficultyOf(current, hasHiddenMod) * skillMultiplier;

            return currentStrain;
        }
    }
}