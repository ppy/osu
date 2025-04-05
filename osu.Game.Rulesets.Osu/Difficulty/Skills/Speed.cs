// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private double totalMultiplier => 1.0;
        private double burstMultiplier => 1.91;
        private double streamMultiplier => 0.15;
        private double staminaMultiplier => 0.05;
        private double meanFactor => 1.25;

        private double currentBurstStrain;
        private double currentStreamStrain;
        private double currentStaminaStrain;
        private double currentRhythm;

        public readonly bool WithoutStamina;

        public Speed(Mod[] mods, bool withoutStamina)
            : base(mods)
        {
            WithoutStamina = withoutStamina;
        }

        private double strainDecayBurst(double ms) => Math.Pow(0.14, ms / 1000);
        private double strainDecayStream(double ms) => Math.Pow(0.01, Math.Pow(ms / 1000, 1.6));

        private double strainDecayStamina(double ms, double staminaValue)
        {
            double changeFactor = currentStaminaStrain > 0 ? 1 + Math.Pow(currentStaminaStrain / (staminaValue + currentStaminaStrain), 25) : 1;
            return Math.Pow(0.01, Math.Pow(ms * changeFactor / 1000, 3.5));

            //double changeFactor = currentStaminaStrain > 0 ? 0.25 + 0.75 * (1 - Math.Pow(1 / (((staminaValue + currentStaminaStrain) / currentStaminaStrain) + 1), 50)) : 1;
            //return Math.Pow(0.1, Math.Pow(ms / 1000, 2.6)) * changeFactor;
        }

        protected override double CalculateInitialStrain(double time, DifficultyHitObject current)
        {
            if (WithoutStamina)
                return currentBurstStrain * currentRhythm * strainDecayBurst(time - current.Previous(0).StartTime);

            return Math.Pow(
                Math.Pow(currentBurstStrain * currentRhythm * strainDecayBurst(time - current.Previous(0).StartTime), meanFactor) +
                Math.Pow(currentStreamStrain * strainDecayStream(time - current.Previous(0).StartTime), meanFactor) +
                Math.Pow(currentStaminaStrain * strainDecayStamina(time - current.Previous(0).StartTime, currentStaminaStrain), meanFactor), 1.0 / meanFactor
            );
        }

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            currentBurstStrain *= strainDecayBurst(((OsuDifficultyHitObject)current).StrainTime);
            currentRhythm = RhythmEvaluator.EvaluateDifficultyOf(current);
            currentBurstStrain += SpeedEvaluator.EvaluateDifficultyOf(current, Mods) * burstMultiplier;

            if (WithoutStamina)
                return currentBurstStrain * currentRhythm;

            double staminaValue = StaminaEvaluator.EvaluateDifficultyOf(current);

            currentStreamStrain *= strainDecayStream(((OsuDifficultyHitObject)current).StrainTime);
            currentStreamStrain += staminaValue * streamMultiplier;

            currentStaminaStrain *= strainDecayStamina(((OsuDifficultyHitObject)current).StrainTime, staminaValue * staminaMultiplier);
            currentStaminaStrain += staminaValue * staminaMultiplier;

            double totalValue =
                Math.Pow(
                    Math.Pow(currentBurstStrain * currentRhythm, meanFactor) +
                    Math.Pow(currentStreamStrain, meanFactor) +
                    Math.Pow(currentStaminaStrain, meanFactor), 1.0 / meanFactor
                );

            return totalValue * totalMultiplier;
        }

        public double RelevantNoteCount()
        {
            if (ObjectStrains.Count == 0)
                return 0;

            double maxStrain = ObjectStrains.Max();
            if (maxStrain == 0)
                return 0;

            return ObjectStrains.Sum(strain => 1.0 / (1.0 + Math.Exp(-(strain / maxStrain * 12.0 - 6.0))));
        }
    }
}
