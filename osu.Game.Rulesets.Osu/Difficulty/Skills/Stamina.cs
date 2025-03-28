// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Stamina : StrainSkill
    {
        private double skillMultiplier => 0.04 * 2;
        private double strainDecayBase => 0.1;
        private double currentStrain;

        public Stamina(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, Math.Pow(ms / 1000, 2.6));

        private double strainDecayStamina(double ms, double staminaValue)
        {
            double changeFactor = currentStrain > 0 ? 1 + Math.Pow(currentStrain / (staminaValue + currentStrain), 20) : 1;
            return Math.Pow(0.01, Math.Pow(ms * changeFactor / 1000, 3.5));
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            var value = StaminaEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;
            //currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrain *= strainDecayStamina(((OsuDifficultyHitObject)current).StrainTime, value);
            currentStrain += value;

            return currentStrain;
        }
    }

    public class StreamStamina : StrainSkill
    {
        private double skillMultiplier => 0.1 * 2;
        private double strainDecayBase => 0.01;
        private double currentStrain;

        public StreamStamina(Mod[] mods)
            : base(mods)
        {
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, Math.Pow(ms / 1000, 1.6));

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current) => (currentStrain) * strainDecay(time - current.Previous(0).StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentStrain *= strainDecay(((OsuDifficultyHitObject)current).StrainTime);
            currentStrain += StaminaEvaluator.EvaluateDifficultyOf(current) * skillMultiplier;

            return currentStrain;
        }
    }
}
