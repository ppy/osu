// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Skills.Pre;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        private double skillMultiplier => 23.25;
        private double strainDecayBase => 0.15;

        private double currentStrain;

        private readonly AimVelocity aimVelocity;

        private readonly AimAngleBonus aimAngleBonus;

        public Aim(Mod[] mods, bool withSliders)
            : base(mods)
        {
            aimVelocity = new AimVelocity(mods, withSliders);
            aimAngleBonus = new AimAngleBonus(mods, withSliders, aimVelocity);
        }

        private double strainValueOf(int index, DifficultyHitObject current)
        {
            aimVelocity.ProcessInternal(index, current);
            aimAngleBonus.ProcessInternal(index, current);

            double aimStrain = aimVelocity[index] + aimAngleBonus[index]; // Start strain with regular velocity.

            return aimStrain;
        }

        //private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => currentStrain * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(int index, DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(index, current) * skillMultiplier;

            return currentStrain;
        }
    }
}
