// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        public Aim(Mod[] mods, bool withSliders)
            : base(mods)
        {
            this.withSliders = withSliders;
        }

        private readonly bool withSliders;

        protected double CurrentStrain;
        protected double SkillMultiplier => 23.55;

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => CurrentStrain * StrainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            CurrentStrain *= StrainDecay(current.DeltaTime);
            CurrentStrain += AimEvaluator.EvaluateDifficultyOf(current, withSliders) * SkillMultiplier;

            return CurrentStrain;
        }
    }
}
