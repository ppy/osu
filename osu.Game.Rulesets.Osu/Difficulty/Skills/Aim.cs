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

        public static double MaxDifficulty; //Max difficulty of a single object

        public static double SumDifficulty; //Summed difficulty of a single object

        private readonly bool withSliders;

        private double currentStrain;

        private double maxDifficulty;

        private double sumDifficulty;

        private double skillMultiplier => 25.18;
        private double strainDecayBase => 0.15;

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => currentStrain * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(current.DeltaTime);

            double currentHitObjectStrain = AimEvaluator.EvaluateDifficultyOf(current, withSliders) * skillMultiplier;

            currentStrain += currentHitObjectStrain;

            sumDifficulty += currentHitObjectStrain;

            if (maxDifficulty < currentHitObjectStrain)
                maxDifficulty = currentHitObjectStrain;

            if (current.Next(1) is null)
            {
                MaxDifficulty = maxDifficulty;
                SumDifficulty = sumDifficulty;
            }

            return currentStrain;
        }
    }
}
